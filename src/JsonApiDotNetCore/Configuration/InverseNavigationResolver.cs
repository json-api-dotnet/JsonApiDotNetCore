using System.Collections.Generic;
using System.Linq;
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
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly IEnumerable<IDbContextResolver> _dbContextResolvers;

        public InverseNavigationResolver(IResourceContextProvider resourceContextProvider, IEnumerable<IDbContextResolver> dbContextResolvers)
        {
            ArgumentGuard.NotNull(resourceContextProvider, nameof(resourceContextProvider));
            ArgumentGuard.NotNull(dbContextResolvers, nameof(dbContextResolvers));

            _resourceContextProvider = resourceContextProvider;
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
            foreach (ResourceContext resourceContext in _resourceContextProvider.GetResourceContexts().Where(context => context.Relationships.Any()))
            {
                IEntityType entityType = dbContext.Model.FindEntityType(resourceContext.ResourceType);

                if (entityType != null)
                {
                    IDictionary<string, INavigationBase> navigationMap = GetNavigations(entityType);
                    ResolveRelationships(resourceContext.Relationships, navigationMap);
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
                if (navigationMap.TryGetValue(relationship.Property.Name, out INavigationBase navigation))
                {
                    relationship.InverseNavigationProperty = navigation.Inverse?.PropertyInfo;
                }
            }
        }
    }
}
