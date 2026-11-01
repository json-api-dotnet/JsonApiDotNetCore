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

        Expression? countExpression = GetCollectionCount(collectionExpression);

        if (countExpression == null)
        {
            throw new InvalidOperationException($"Field '{expression.TargetCollection}' must be a collection.");
        }

        if (RequiresNullGuard(expression.TargetCollection, collectionExpression))
        {
            // A null collection (or a collection below a null compound) counts as empty.
            return Expression.Condition(IsNotNull(collectionExpression), countExpression, Expression.Constant(0));
        }

        return countExpression;
    }

    /// <summary>
    /// Determines whether the specified collection expression can be null, which is the case for a collection attribute of a reference type.
    /// </summary>
    protected static bool RequiresNullGuard(ResourceFieldChainExpression targetCollection, Expression collectionExpression)
    {
        ArgumentNullException.ThrowIfNull(targetCollection);
        ArgumentNullException.ThrowIfNull(collectionExpression);

        return targetCollection.Fields[^1] is AttrAttribute && !collectionExpression.Type.IsValueType;
    }

    /// <summary>
    /// Produces a null check for the specified collection expression.
    /// </summary>
    protected static BinaryExpression IsNotNull(Expression collectionExpression)
    {
        ArgumentNullException.ThrowIfNull(collectionExpression);

        return Expression.NotEqual(collectionExpression, Expression.Constant(null, collectionExpression.Type));
    }

    private static Expression? GetCollectionCount(Expression? collectionExpression)
    {
        if (collectionExpression is { Type.IsArray: true })
        {
            // Matches what the C# compiler emits for 'array.Length'. Query providers recognize this node type (for example, Npgsql
            // translates it to length(bytea) for byte[]), but not a member access on the Array.Length property.
            return Expression.ArrayLength(collectionExpression);
        }

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
