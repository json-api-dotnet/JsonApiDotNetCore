using JetBrains.Annotations;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JsonApiDotNetCore.Configuration;

/// <inheritdoc cref="IInverseNavigationResolver" />
[PublicAPI]
public sealed class InverseNavigationResolver : IInverseNavigationResolver
{
    private readonly IResourceGraph _resourceGraph;
    private readonly IDbContextResolver[] _dbContextResolvers;

    public InverseNavigationResolver(IResourceGraph resourceGraph, IEnumerable<IDbContextResolver> dbContextResolvers)
    {
        ArgumentGuard.NotNull(resourceGraph);
        ArgumentGuard.NotNull(dbContextResolvers);

        _resourceGraph = resourceGraph;
        _dbContextResolvers = dbContextResolvers as IDbContextResolver[] ?? dbContextResolvers.ToArray();
    }

    /// <inheritdoc />
    public void Resolve()
    {
        foreach (IDbContextResolver dbContextResolver in _dbContextResolvers)
        {
            DbContext dbContext = dbContextResolver.GetContext();
            Resolve(dbContext);
        }
    }

    private void Resolve(DbContext dbContext)
    {
        foreach (ResourceType resourceType in _resourceGraph.GetResourceTypes().Where(resourceType => resourceType.Relationships.Count > 0))
        {
            IEntityType? entityType = dbContext.Model.FindEntityType(resourceType.ClrType);

            if (entityType != null)
            {
                Dictionary<string, INavigationBase> navigationMap = GetNavigations(entityType);
                ResolveRelationships(resourceType.Relationships, navigationMap);
            }
        }
    }

    private static Dictionary<string, INavigationBase> GetNavigations(IEntityType entityType)
    {
        // @formatter:wrap_chained_method_calls chop_always

        return entityType.GetNavigations()
            .Cast<INavigationBase>()
            .Concat(entityType.GetSkipNavigations())
            .ToDictionary(navigation => navigation.Name);

        // @formatter:wrap_chained_method_calls restore
    }

    private void ResolveRelationships(IReadOnlyCollection<RelationshipAttribute> relationships, Dictionary<string, INavigationBase> navigationMap)
    {
        foreach (RelationshipAttribute relationship in relationships)
        {
            if (navigationMap.TryGetValue(relationship.Property.Name, out INavigationBase? navigation))
            {
                relationship.InverseNavigationProperty = navigation.Inverse?.PropertyInfo;
            }
        }
    }
}
