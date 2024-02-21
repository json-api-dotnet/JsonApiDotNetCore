using System.Collections;
using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.QueryableBuilding;

/// <inheritdoc cref="IWhereClauseBuilder" />
[PublicAPI]
public class WhereClauseBuilder : QueryClauseBuilder, IWhereClauseBuilder
{
    private static readonly CollectionConverter CollectionConverter = new();
    private static readonly ConstantExpression NullConstant = Expression.Constant(null);

    public virtual Expression ApplyWhere(FilterExpression filter, QueryClauseBuilderContext context)
    {
        ArgumentGuard.NotNull(filter);

        LambdaExpression lambda = GetPredicateLambda(filter, context);

        return WhereExtensionMethodCall(lambda, context);
    }

    private LambdaExpression GetPredicateLambda(FilterExpression filter, QueryClauseBuilderContext context)
    {
        Expression body = Visit(filter, context);
        return Expression.Lambda(body, context.LambdaScope.Parameter);
    }

    private static Expression WhereExtensionMethodCall(LambdaExpression predicate, QueryClauseBuilderContext context)
    {
        return Expression.Call(context.ExtensionType, "Where", [context.LambdaScope.Parameter.Type], context.Source, predicate);
    }

    public override Expression VisitHas(HasExpression expression, QueryClauseBuilderContext context)
    {
        Expression property = Visit(expression.TargetCollection, context);

        Type? elementType = CollectionConverter.FindCollectionElementType(property.Type);

        if (elementType == null)
        {
            throw new InvalidOperationException("Expression must be a collection.");
        }

        Expression? predicate = null;

        if (expression.Filter != null)
        {
            ResourceType resourceType = ((HasManyAttribute)expression.TargetCollection.Fields[^1]).RightType;

            using LambdaScope lambdaScope = context.LambdaScopeFactory.CreateScope(elementType);

            var nestedContext = new QueryClauseBuilderContext(property, resourceType, typeof(Enumerable), context.EntityModel, context.LambdaScopeFactory,
                lambdaScope, context.QueryableBuilder, context.State);

            predicate = GetPredicateLambda(expression.Filter, nestedContext);
        }

        return AnyExtensionMethodCall(elementType, property, predicate);
    }

    private static MethodCallExpression AnyExtensionMethodCall(Type elementType, Expression source, Expression? predicate)
    {
        return predicate != null
            ? Expression.Call(typeof(Enumerable), "Any", [elementType], source, predicate)
            : Expression.Call(typeof(Enumerable), "Any", [elementType], source);
    }

    public override Expression VisitIsType(IsTypeExpression expression, QueryClauseBuilderContext context)
    {
        Expression property = expression.TargetToOneRelationship != null ? Visit(expression.TargetToOneRelationship, context) : context.LambdaScope.Accessor;
        TypeBinaryExpression typeCheck = Expression.TypeIs(property, expression.DerivedType.ClrType);

        if (expression.Child == null)
        {
            return typeCheck;
        }

        UnaryExpression derivedAccessor = Expression.Convert(property, expression.DerivedType.ClrType);

        QueryClauseBuilderContext derivedContext = context.WithLambdaScope(context.LambdaScope.WithAccessor(derivedAccessor));
        Expression filter = Visit(expression.Child, derivedContext);

        return Expression.AndAlso(typeCheck, filter);
    }

    public override Expression VisitMatchText(MatchTextExpression expression, QueryClauseBuilderContext context)
    {
        Expression property = Visit(expression.TargetAttribute, context);

        if (property.Type != typeof(string))
        {
            throw new InvalidOperationException("Expression must be a string.");
        }

        Expression text = Visit(expression.TextValue, context);

        return expression.MatchKind switch
        {
            TextMatchKind.StartsWith => Expression.Call(property, "StartsWith", null, text),
            TextMatchKind.EndsWith => Expression.Call(property, "EndsWith", null, text),
            _ => Expression.Call(property, "Contains", null, text)
        };
    }

    public override Expression VisitAny(AnyExpression expression, QueryClauseBuilderContext context)
    {
        Expression property = Visit(expression.TargetAttribute, context);

        var valueList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(property.Type))!;

        foreach (LiteralConstantExpression constant in expression.Constants)
        {
            valueList.Add(constant.TypedValue);
        }

        ConstantExpression collection = Expression.Constant(valueList);
        return ContainsExtensionMethodCall(collection, property);
    }

    private static Expression ContainsExtensionMethodCall(Expression collection, Expression value)
    {
        return Expression.Call(typeof(Enumerable), "Contains", [value.Type], collection, value);
    }

    public override Expression VisitLogical(LogicalExpression expression, QueryClauseBuilderContext context)
    {
        var termQueue = new Queue<Expression>(expression.Terms.Select(filter => Visit(filter, context)));

        return expression.Operator switch
        {
            LogicalOperator.And => Compose(termQueue, Expression.AndAlso),
            LogicalOperator.Or => Compose(termQueue, Expression.OrElse),
            _ => throw new InvalidOperationException($"Unknown logical operator '{expression.Operator}'.")
        };
    }

    private static BinaryExpression Compose(Queue<Expression> argumentQueue, Func<Expression, Expression, BinaryExpression> applyOperator)
    {
        Expression left = argumentQueue.Dequeue();
        Expression right = argumentQueue.Dequeue();

        BinaryExpression tempExpression = applyOperator(left, right);

        while (argumentQueue.Any())
        {
            Expression nextArgument = argumentQueue.Dequeue();
            tempExpression = applyOperator(tempExpression, nextArgument);
        }

        return tempExpression;
    }

    public override Expression VisitNot(NotExpression expression, QueryClauseBuilderContext context)
    {
        Expression child = Visit(expression.Child, context);
        return Expression.Not(child);
    }

    public override Expression VisitComparison(ComparisonExpression expression, QueryClauseBuilderContext context)
    {
        Type commonType = ResolveCommonType(expression.Left, expression.Right, context);

        Expression left = WrapInConvert(Visit(expression.Left, context), commonType);
        Expression right = WrapInConvert(Visit(expression.Right, context), commonType);

        return expression.Operator switch
        {
            ComparisonOperator.Equals => Expression.Equal(left, right),
            ComparisonOperator.LessThan => Expression.LessThan(left, right),
            ComparisonOperator.LessOrEqual => Expression.LessThanOrEqual(left, right),
            ComparisonOperator.GreaterThan => Expression.GreaterThan(left, right),
            ComparisonOperator.GreaterOrEqual => Expression.GreaterThanOrEqual(left, right),
            _ => throw new InvalidOperationException($"Unknown comparison operator '{expression.Operator}'.")
        };
    }

    private Type ResolveCommonType(QueryExpression left, QueryExpression right, QueryClauseBuilderContext context)
    {
        Type leftType = ResolveFixedType(left, context);

        if (RuntimeTypeConverter.CanContainNull(leftType))
        {
            return leftType;
        }

        if (right is NullConstantExpression)
        {
            return typeof(Nullable<>).MakeGenericType(leftType);
        }

        Type? rightType = TryResolveFixedType(right, context);

        if (rightType != null && RuntimeTypeConverter.CanContainNull(rightType))
        {
            return rightType;
        }

        return leftType;
    }

    private Type ResolveFixedType(QueryExpression expression, QueryClauseBuilderContext context)
    {
        Expression result = Visit(expression, context);
        return result.Type;
    }

    private Type? TryResolveFixedType(QueryExpression expression, QueryClauseBuilderContext context)
    {
        if (expression is CountExpression)
        {
            return typeof(int);
        }

        if (expression is ResourceFieldChainExpression chain)
        {
            Expression child = Visit(chain, context);
            return child.Type;
        }

        return null;
    }

    private static Expression WrapInConvert(Expression expression, Type targetType)
    {
        try
        {
            return expression.Type != targetType ? Expression.Convert(expression, targetType) : expression;
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidQueryException("Query creation failed due to incompatible types.", exception);
        }
    }

    public override Expression VisitNullConstant(NullConstantExpression expression, QueryClauseBuilderContext context)
    {
        return NullConstant;
    }

    public override Expression VisitLiteralConstant(LiteralConstantExpression expression, QueryClauseBuilderContext context)
    {
        return SystemExpressionBuilder.CloseOver(expression.TypedValue);
    }
}
