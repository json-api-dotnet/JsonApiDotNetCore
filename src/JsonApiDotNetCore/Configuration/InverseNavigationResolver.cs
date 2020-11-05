using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JsonApiDotNetCore.Configuration
{
    /// <inheritdoc />
    public class InverseNavigationResolver : IInverseNavigationResolver
    {
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly IEnumerable<IDbContextResolver> _dbContextResolvers;

        public InverseNavigationResolver(IResourceContextProvider resourceContextProvider,
            IEnumerable<IDbContextResolver> dbContextResolvers)
        {
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
            _dbContextResolvers = dbContextResolvers ?? throw new ArgumentNullException(nameof(dbContextResolvers));
        }

        /// <inheritdoc />
        public void Resolve()
        {
            foreach (var dbContextResolver in _dbContextResolvers)
            {
                DbContext dbContext = dbContextResolver.GetContext();
                Resolve(dbContext);
            }
        }

        private void Resolve(DbContext dbContext)
        {
            foreach (ResourceContext resourceContext in _resourceContextProvider.GetResourceContexts())
            {
                IEntityType entityType = dbContext.Model.FindEntityType(resourceContext.ResourceType);
                if (entityType != null)
                {
                    foreach (var relationship in resourceContext.Relationships)
                    {
                        if (!(relationship is HasManyThroughAttribute))
                        {
                            INavigation inverseNavigation = entityType.FindNavigation(relationship.Property.Name)?.FindInverse();
                            relationship.InverseNavigationProperty = inverseNavigation?.PropertyInfo;
                        }
                    }
                }
            }
        }
    }
}
