using JetBrains.Annotations;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JsonApiDotNetCore.Configuration
{
    /// <inheritdoc />
    [PublicAPI]
    public sealed class InverseNavigationResolver : IInverseNavigationResolver
    {
        private readonly IResourceGraph _resourceGraph;
        private readonly IEnumerable<IDbContextResolver> _dbContextResolvers;

        public InverseNavigationResolver(IResourceGraph resourceGraph, IEnumerable<IDbContextResolver> dbContextResolvers)
        {
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));
            ArgumentGuard.NotNull(dbContextResolvers, nameof(dbContextResolvers));

            _resourceGraph = resourceGraph;
            _dbContextResolvers = dbContextResolvers;
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
            foreach (ResourceType resourceType in _resourceGraph.GetResourceTypes().Where(resourceType => resourceType.Relationships.Any()))
            {
                IEntityType? entityType = dbContext.Model.FindEntityType(resourceType.ClrType);

                if (entityType != null)
                {
                    IDictionary<string, INavigationBase> navigationMap = GetNavigations(entityType);
                    ResolveRelationships(resourceType.Relationships, navigationMap);
                }
            }
        }

        private static IDictionary<string, INavigationBase> GetNavigations(IEntityType entityType)
        {
            // @formatter:wrap_chained_method_calls chop_always

            return entityType.GetNavigations()
                .Cast<INavigationBase>()
                .Concat(entityType.GetSkipNavigations())
                .ToDictionary(navigation => navigation.Name);

            // @formatter:wrap_chained_method_calls restore
        }

        private void ResolveRelationships(IReadOnlyCollection<RelationshipAttribute> relationships, IDictionary<string, INavigationBase> navigationMap)
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
}
