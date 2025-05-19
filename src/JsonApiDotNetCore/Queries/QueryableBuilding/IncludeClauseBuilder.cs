using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Queries.QueryableBuilding;

/// <inheritdoc cref="IIncludeClauseBuilder" />
[PublicAPI]
public class IncludeClauseBuilder : QueryClauseBuilder, IIncludeClauseBuilder
{
    public virtual Expression ApplyInclude(IncludeExpression include, QueryClauseBuilderContext context)
    {
        ArgumentNullException.ThrowIfNull(include);

        return Visit(include, context);
    }

    public override Expression VisitInclude(IncludeExpression expression, QueryClauseBuilderContext context)
    {
        // De-duplicate chains coming from derived relationships.
        HashSet<string> propertyPaths = [];

        if (context.FieldContainer is ResourceType resourceType)
        {
            ApplyEagerLoads(resourceType.EagerLoads, null, propertyPaths);
        }

        foreach (ResourceFieldChainExpression chain in IncludeChainConverter.Instance.GetRelationshipChains(expression))
        {
            ProcessRelationshipChain(chain, propertyPaths);
        }

        return ToExpression(context.Source, context.LambdaScope.Parameter.Type, propertyPaths);
    }

    private static void ProcessRelationshipChain(ResourceFieldChainExpression chain, HashSet<string> outputPropertyPaths)
    {
        string? path = null;

        foreach (RelationshipAttribute relationship in chain.Fields.Cast<RelationshipAttribute>())
        {
            path = path == null ? relationship.Property.Name : $"{path}.{relationship.Property.Name}";

            ApplyEagerLoads(relationship.RightType.EagerLoads, path, outputPropertyPaths);
        }

        outputPropertyPaths.Add(path!);
    }

    private static void ApplyEagerLoads(IEnumerable<EagerLoadAttribute> eagerLoads, string? pathPrefix, ISet<string> outputPropertyPaths)
    {
        foreach (EagerLoadAttribute eagerLoad in eagerLoads)
        {
            string path = pathPrefix != null ? $"{pathPrefix}.{eagerLoad.Property.Name}" : eagerLoad.Property.Name;
            outputPropertyPaths.Add(path);

            ApplyEagerLoads(eagerLoad.Children, path, outputPropertyPaths);
        }
    }

    private static Expression ToExpression(Expression source, Type entityType, HashSet<string> propertyPaths)
    {
        Expression expression = source;

        foreach (string propertyPath in propertyPaths)
        {
            expression = IncludeExtensionMethodCall(expression, entityType, propertyPath);
        }

        return expression;
    }

    private static MethodCallExpression IncludeExtensionMethodCall(Expression source, Type entityType, string navigationPropertyPath)
    {
        Expression navigationExpression = Expression.Constant(navigationPropertyPath);

        return Expression.Call(typeof(EntityFrameworkQueryableExtensions), "Include", [entityType], source, navigationExpression);
    }
}
