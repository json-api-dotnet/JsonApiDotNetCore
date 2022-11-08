using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Resources;

internal sealed class SortExpressionLambdaConverter
{
    private readonly IResourceGraph _resourceGraph;
    private readonly IList<ResourceFieldAttribute> _fields = new List<ResourceFieldAttribute>();

    public SortExpressionLambdaConverter(IResourceGraph resourceGraph)
    {
        ArgumentGuard.NotNull(resourceGraph);

        _resourceGraph = resourceGraph;
    }

    public SortElementExpression FromLambda<TResource>(Expression<Func<TResource, object?>> keySelector, ListSortDirection sortDirection)
    {
        ArgumentGuard.NotNull(keySelector);

        _fields.Clear();

        Expression lambdaBodyExpression = SkipConvert(keySelector.Body);
        (Expression? expression, bool isCount) = TryReadCount(lambdaBodyExpression);

        if (expression != null)
        {
            expression = SkipConvert(expression);
            expression = isCount ? ReadToManyRelationship(expression) : ReadAttribute(expression);

            while (expression != null)
            {
                expression = SkipConvert(expression);

                if (IsLambdaParameter(expression, keySelector.Parameters[0]))
                {
                    return ToSortElement(isCount, sortDirection);
                }

                expression = ReadToOneRelationship(expression);
            }
        }

        throw new InvalidOperationException($"Unsupported expression body '{lambdaBodyExpression}'.");
    }

    private static Expression SkipConvert(Expression expression)
    {
        Expression inner = expression;

        while (true)
        {
            if (inner is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.TypeAs } unary)
            {
                inner = unary.Operand;
            }
            else
            {
                return inner;
            }
        }
    }

    private static (Expression? innerExpression, bool isCount) TryReadCount(Expression expression)
    {
        if (expression is MethodCallExpression methodCallExpression && methodCallExpression.Method.Name == "Count")
        {
            if (methodCallExpression.Arguments.Count <= 1)
            {
                return (methodCallExpression.Arguments[0], true);
            }

            throw new InvalidOperationException("Count method that takes a predicate is not supported.");
        }

        if (expression is MemberExpression memberExpression)
        {
            if (memberExpression.Member.MemberType == MemberTypes.Property && memberExpression.Member.Name is "Count" or "Length")
            {
                if (memberExpression.Member.GetCustomAttribute<AttrAttribute>() == null)
                {
                    return (memberExpression.Expression, true);
                }
            }

            return (memberExpression, false);
        }

        return (null, false);
    }

    private Expression? ReadToManyRelationship(Expression expression)
    {
        if (expression is MemberExpression memberExpression)
        {
            ResourceType resourceType = _resourceGraph.GetResourceType(memberExpression.Member.DeclaringType!);
            RelationshipAttribute? relationship = resourceType.FindRelationshipByPropertyName(memberExpression.Member.Name);

            if (relationship is HasManyAttribute)
            {
                _fields.Insert(0, relationship);
                return memberExpression.Expression;
            }
        }

        throw new InvalidOperationException($"Expected property for JSON:API to-many relationship, but found '{expression}'.");
    }

    private Expression? ReadAttribute(Expression expression)
    {
        if (expression is MemberExpression { Expression: { } } memberExpression)
        {
            ResourceType resourceType = _resourceGraph.GetResourceType(memberExpression.Expression.Type);
            AttrAttribute? attribute = resourceType.FindAttributeByPropertyName(memberExpression.Member.Name);

            if (attribute != null)
            {
                _fields.Insert(0, attribute);
                return memberExpression.Expression;
            }
        }

        throw new InvalidOperationException($"Expected property for JSON:API attribute, but found '{expression}'.");
    }

    private Expression? ReadToOneRelationship(Expression expression)
    {
        if (expression is MemberExpression memberExpression)
        {
            ResourceType resourceType = _resourceGraph.GetResourceType(memberExpression.Member.DeclaringType!);
            RelationshipAttribute? relationship = resourceType.FindRelationshipByPropertyName(memberExpression.Member.Name);

            if (relationship is HasOneAttribute)
            {
                _fields.Insert(0, relationship);
                return memberExpression.Expression;
            }
        }

        throw new InvalidOperationException($"Expected property for JSON:API to-one relationship, but found '{expression}'.");
    }

    private static bool IsLambdaParameter(Expression expression, ParameterExpression lambdaParameter)
    {
        return expression is ParameterExpression parameterExpression && parameterExpression.Name == lambdaParameter.Name;
    }

    private SortElementExpression ToSortElement(bool isCount, ListSortDirection sortDirection)
    {
        var chain = new ResourceFieldChainExpression(_fields.ToImmutableArray());
        bool isAscending = sortDirection == ListSortDirection.Ascending;

        if (isCount)
        {
            var countExpression = new CountExpression(chain);
            return new SortElementExpression(countExpression, isAscending);
        }

        return new SortElementExpression(chain, isAscending);
    }
}
