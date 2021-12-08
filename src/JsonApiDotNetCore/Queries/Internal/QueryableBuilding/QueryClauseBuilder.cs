using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCore.Queries.Internal.QueryableBuilding;

/// <summary>
/// Base class for transforming <see cref="QueryExpression" /> trees into system <see cref="Expression" /> trees.
/// </summary>
public abstract class QueryClauseBuilder<TArgument> : QueryExpressionVisitor<TArgument, Expression>
{
    protected LambdaScope LambdaScope { get; }

    protected QueryClauseBuilder(LambdaScope lambdaScope)
    {
        ArgumentGuard.NotNull(lambdaScope, nameof(lambdaScope));

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
        string[] components = expression.Fields.Select(field => field.Property.Name).ToArray();

        return CreatePropertyExpressionFromComponents(LambdaScope.Accessor, components);
    }

    private static MemberExpression CreatePropertyExpressionFromComponents(Expression source, IEnumerable<string> components)
    {
        MemberExpression? property = null;

        foreach (string propertyName in components)
        {
            Type parentType = property == null ? source.Type : property.Type;

            if (parentType.GetProperty(propertyName) == null)
            {
                throw new InvalidOperationException($"Type '{parentType.Name}' does not contain a property named '{propertyName}'.");
            }

            property = property == null ? Expression.Property(source, propertyName) : Expression.Property(property, propertyName);
        }

        return property!;
    }
}
