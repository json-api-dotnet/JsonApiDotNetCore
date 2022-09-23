using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.QueryableBuilding;

/// <summary>
/// Base class for transforming <see cref="QueryExpression" /> trees into system <see cref="Expression" /> trees.
/// </summary>
public abstract class QueryClauseBuilder<TArgument> : QueryExpressionVisitor<TArgument, Expression>
{
    protected LambdaScope LambdaScope { get; private set; }

    protected QueryClauseBuilder(LambdaScope lambdaScope)
    {
        ArgumentGuard.NotNull(lambdaScope);

        LambdaScope = lambdaScope;
    }

    public override Expression VisitCount(CountExpression expression, TArgument argument)
    {
        Expression collectionExpression = Visit(expression.TargetCollection, argument);

        Expression? propertyExpression = GetCollectionCount(collectionExpression);

        if (propertyExpression == null)
        {
            throw new InvalidOperationException($"Field '{expression.TargetCollection}' must be a collection.");
        }

        return propertyExpression;
    }

    private static Expression? GetCollectionCount(Expression? collectionExpression)
    {
        if (collectionExpression != null)
        {
            var properties = new HashSet<PropertyInfo>(collectionExpression.Type.GetProperties());

            if (collectionExpression.Type.IsInterface)
            {
                foreach (PropertyInfo item in collectionExpression.Type.GetInterfaces().SelectMany(@interface => @interface.GetProperties()))
                {
                    properties.Add(item);
                }
            }

            foreach (PropertyInfo property in properties)
            {
                if (property.Name is "Count" or "Length")
                {
                    return Expression.Property(collectionExpression, property);
                }
            }
        }

        return null;
    }

    public override Expression VisitResourceFieldChain(ResourceFieldChainExpression expression, TArgument argument)
    {
        MemberExpression? property = null;

        foreach (ResourceFieldAttribute field in expression.Fields)
        {
            Expression parentAccessor = property ?? LambdaScope.Accessor;
            Type propertyType = field.Property.DeclaringType!;
            string propertyName = field.Property.Name;

            bool requiresUpCast = parentAccessor.Type != propertyType && parentAccessor.Type.IsAssignableFrom(propertyType);
            Type parentType = requiresUpCast ? propertyType : parentAccessor.Type;

            if (parentType.GetProperty(propertyName) == null)
            {
                throw new InvalidOperationException($"Type '{parentType.Name}' does not contain a property named '{propertyName}'.");
            }

            property = requiresUpCast
                ? Expression.MakeMemberAccess(Expression.Convert(parentAccessor, propertyType), field.Property)
                : Expression.Property(parentAccessor, propertyName);
        }

        return property!;
    }

    protected TResult WithLambdaScopeAccessor<TResult>(Expression accessorExpression, Func<TResult> action)
    {
        ArgumentGuard.NotNull(accessorExpression);
        ArgumentGuard.NotNull(action);

        LambdaScope backupScope = LambdaScope;

        try
        {
            using (LambdaScope = LambdaScope.WithAccessor(accessorExpression))
            {
                return action();
            }
        }
        finally
        {
            LambdaScope = backupScope;
        }
    }
}
