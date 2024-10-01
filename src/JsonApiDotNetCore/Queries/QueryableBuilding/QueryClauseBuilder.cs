using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.QueryableBuilding;

/// <summary>
/// Base class for transforming <see cref="QueryExpression" /> trees into system <see cref="Expression" /> trees.
/// </summary>
public abstract class QueryClauseBuilder : QueryExpressionVisitor<QueryClauseBuilderContext, Expression>
{
    public override Expression DefaultVisit(QueryExpression expression, QueryClauseBuilderContext argument)
    {
        throw new NotSupportedException($"Unknown expression of type '{expression.GetType()}'.");
    }

    public override Expression VisitCount(CountExpression expression, QueryClauseBuilderContext context)
    {
        Expression collectionExpression = Visit(expression.TargetCollection, context);

        MemberExpression? propertyExpression = GetCollectionCount(collectionExpression);

        if (propertyExpression == null)
        {
            throw new InvalidOperationException($"Field '{expression.TargetCollection}' must be a collection.");
        }

        return propertyExpression;
    }

    private static MemberExpression? GetCollectionCount(Expression? collectionExpression)
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

    public override Expression VisitResourceFieldChain(ResourceFieldChainExpression expression, QueryClauseBuilderContext context)
    {
        MemberExpression? property = null;

        foreach (ResourceFieldAttribute field in expression.Fields)
        {
            Expression parentAccessor = property ?? context.LambdaScope.Accessor;
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
}
