using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Repositories
{
    /// <inheritdoc />
    public class ResourceRepositoryAccessor : IResourceRepositoryAccessor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IResourceContextProvider _resourceContextProvider;

        public ResourceRepositoryAccessor(IServiceProvider serviceProvider, IResourceContextProvider resourceContextProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentException(nameof(serviceProvider));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentException(nameof(serviceProvider));
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<TResource>> GetAsync<TResource>(QueryLayer layer)
            where TResource : class, IIdentifiable
        {
            dynamic repository = GetReadRepository(typeof(TResource));
            return (IReadOnlyCollection<TResource>) await repository.GetAsync(layer);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IIdentifiable>> GetAsync(Type resourceType, QueryLayer layer)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));

            dynamic repository = GetReadRepository(resourceType);
            return (IReadOnlyCollection<IIdentifiable>) await repository.GetAsync(layer);
        }

        /// <inheritdoc />
        public async Task<int> CountAsync<TResource>(FilterExpression topFilter)
            where TResource : class, IIdentifiable
        {
            dynamic repository = GetReadRepository(typeof(TResource));
            return (int) await repository.CountAsync(topFilter);
        }

        /// <inheritdoc />
        public async Task CreateAsync<TResource>(TResource resource)
            where TResource : class, IIdentifiable
        {
            dynamic repository = GetWriteRepository(typeof(TResource));
            await repository.CreateAsync(resource);
        }

        /// <inheritdoc />
        public async Task AddToToManyRelationshipAsync<TResource, TId>(TId primaryId, ISet<IIdentifiable> secondaryResourceIds)
            where TResource : class, IIdentifiable<TId>
        {
            dynamic repository = GetWriteRepository(typeof(TResource));
            await repository.AddToToManyRelationshipAsync(primaryId, secondaryResourceIds);
        }

        /// <inheritdoc />
        public async Task UpdateAsync<TResource>(TResource resourceFromRequest, TResource resourceFromDatabase)
            where TResource : class, IIdentifiable
        {
            dynamic repository = GetWriteRepository(typeof(TResource));
            await repository.UpdateAsync(resourceFromRequest, resourceFromDatabase);
        }

        /// <inheritdoc />
        public async Task SetRelationshipAsync<TResource>(TResource primaryResource, object secondaryResourceIds)
            where TResource : class, IIdentifiable
        {
            dynamic repository = GetWriteRepository(typeof(TResource));
            await repository.SetRelationshipAsync(primaryResource, secondaryResourceIds);
        }

        /// <inheritdoc />
        public async Task DeleteAsync<TResource, TId>(TId id)
            where TResource : class, IIdentifiable<TId>
        {
            dynamic repository = GetWriteRepository(typeof(TResource));
            await repository.DeleteAsync(id);
        }

        /// <inheritdoc />
        public async Task RemoveFromToManyRelationshipAsync<TResource>(TResource primaryResource, ISet<IIdentifiable> secondaryResourceIds)
            where TResource : class, IIdentifiable
        {
            dynamic repository = GetWriteRepository(typeof(TResource));
            await repository.RemoveFromToManyRelationshipAsync(primaryResource, secondaryResourceIds);
        }

        /// <inheritdoc />
        public async Task<TResource> GetForUpdateAsync<TResource>(QueryLayer queryLayer)
            where TResource : class, IIdentifiable
        {
            dynamic repository = GetWriteRepository(typeof(TResource));
            return await repository.GetForUpdateAsync(queryLayer);
        }

        protected object GetReadRepository(Type resourceType)
        {
            var resourceContext = _resourceContextProvider.GetResourceContext(resourceType);

            if (resourceContext.IdentityType == typeof(int))
            {
                var intRepositoryType = typeof(IResourceReadRepository<>).MakeGenericType(resourceContext.ResourceType);
                var intRepository = _serviceProvider.GetService(intRepositoryType);

                if (intRepository != null)
                {
                    return intRepository;
                }
            }

            var resourceDefinitionType = typeof(IResourceReadRepository<,>).MakeGenericType(resourceContext.ResourceType, resourceContext.IdentityType);
            return _serviceProvider.GetRequiredService(resourceDefinitionType);
        }

        protected object GetWriteRepository(Type resourceType)
        {
            var resourceContext = _resourceContextProvider.GetResourceContext(resourceType);

            if (resourceContext.IdentityType == typeof(int))
            {
                var intRepositoryType = typeof(IResourceWriteRepository<>).MakeGenericType(resourceContext.ResourceType);
                var intRepository = _serviceProvider.GetService(intRepositoryType);

                if (intRepository != null)
                {
                    return intRepository;
                }
            }

            var resourceDefinitionType = typeof(IResourceWriteRepository<,>).MakeGenericType(resourceContext.ResourceType, resourceContext.IdentityType);
            return _serviceProvider.GetRequiredService(resourceDefinitionType);
        }
    }
}
