using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Queries.Internal.QueryableBuilding;

/// <summary>
/// Transforms <see cref="IncludeExpression" /> into <see cref="EntityFrameworkQueryableExtensions.Include{TEntity, TProperty}" /> calls.
/// </summary>
[PublicAPI]
public class IncludeClauseBuilder : QueryClauseBuilder<object?>
{
    private static readonly IncludeChainConverter IncludeChainConverter = new();

    private readonly Expression _source;
    private readonly ResourceType _resourceType;

    public IncludeClauseBuilder(Expression source, LambdaScope lambdaScope, ResourceType resourceType)
        : base(lambdaScope)
    {
        ArgumentGuard.NotNull(source);
        ArgumentGuard.NotNull(resourceType);

        _source = source;
        _resourceType = resourceType;
    }

    public Expression ApplyInclude(IncludeExpression include)
    {
        ArgumentGuard.NotNull(include);

        return Visit(include, null);
    }

    public override Expression VisitInclude(IncludeExpression expression, object? argument)
    {
        // De-duplicate chains coming from derived relationships.
        HashSet<string> propertyPaths = new();

        ApplyEagerLoads(_resourceType.EagerLoads, null, propertyPaths);

        foreach (ResourceFieldChainExpression chain in IncludeChainConverter.GetRelationshipChains(expression))
        {
            ProcessRelationshipChain(chain, propertyPaths);
        }

        return ToExpression(propertyPaths);
    }

    private void ProcessRelationshipChain(ResourceFieldChainExpression chain, ISet<string> outputPropertyPaths)
    {
        string? path = null;

        foreach (RelationshipAttribute relationship in chain.Fields.Cast<RelationshipAttribute>())
        {
            path = path == null ? relationship.Property.Name : $"{path}.{relationship.Property.Name}";

            ApplyEagerLoads(relationship.RightType.EagerLoads, path, outputPropertyPaths);
        }

        outputPropertyPaths.Add(path!);
    }

    private void ApplyEagerLoads(IEnumerable<EagerLoadAttribute> eagerLoads, string? pathPrefix, ISet<string> outputPropertyPaths)
    {
        foreach (EagerLoadAttribute eagerLoad in eagerLoads)
        {
            string path = pathPrefix != null ? $"{pathPrefix}.{eagerLoad.Property.Name}" : eagerLoad.Property.Name;
            outputPropertyPaths.Add(path);

            ApplyEagerLoads(eagerLoad.Children, path, outputPropertyPaths);
        }
    }

    private Expression ToExpression(HashSet<string> propertyPaths)
    {
        Expression source = _source;

        foreach (string propertyPath in propertyPaths)
        {
            source = IncludeExtensionMethodCall(source, propertyPath);
        }

        return source;
    }

    private Expression IncludeExtensionMethodCall(Expression source, string navigationPropertyPath)
    {
        Expression navigationExpression = Expression.Constant(navigationPropertyPath);

        return Expression.Call(typeof(EntityFrameworkQueryableExtensions), "Include", LambdaScope.Parameter.Type.AsArray(), source, navigationExpression);
    }
}
