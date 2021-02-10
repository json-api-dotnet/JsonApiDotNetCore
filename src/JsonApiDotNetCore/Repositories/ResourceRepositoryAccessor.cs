using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
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
        private readonly IJsonApiRequest _request;

        public ResourceRepositoryAccessor(IServiceProvider serviceProvider, IResourceContextProvider resourceContextProvider, IJsonApiRequest request)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentException(nameof(serviceProvider));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentException(nameof(serviceProvider));
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<TResource>> GetAsync<TResource>(QueryLayer layer, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            dynamic repository = ResolveReadRepository(typeof(TResource));
            return (IReadOnlyCollection<TResource>) await repository.GetAsync(layer, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IIdentifiable>> GetAsync(Type resourceType, QueryLayer layer, CancellationToken cancellationToken)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));

            dynamic repository = ResolveReadRepository(resourceType);
            return (IReadOnlyCollection<IIdentifiable>) await repository.GetAsync(layer, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<int> CountAsync<TResource>(FilterExpression topFilter, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            dynamic repository = ResolveReadRepository(typeof(TResource));
            return (int) await repository.CountAsync(topFilter, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<TResource> GetForCreateAsync<TResource, TId>(TId id, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable<TId>
        {
            dynamic repository = GetWriteRepository(typeof(TResource));
            return await repository.GetForCreateAsync(id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task CreateAsync<TResource>(TResource resourceFromRequest, TResource resourceForDatabase, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            dynamic repository = GetWriteRepository(typeof(TResource));
            await repository.CreateAsync(resourceFromRequest, resourceForDatabase, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<TResource> GetForUpdateAsync<TResource>(QueryLayer queryLayer, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            dynamic repository = GetWriteRepository(typeof(TResource));
            return await repository.GetForUpdateAsync(queryLayer, cancellationToken);
        }

        /// <inheritdoc />
        public async Task UpdateAsync<TResource>(TResource resourceFromRequest, TResource resourceFromDatabase, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            dynamic repository = GetWriteRepository(typeof(TResource));
            await repository.UpdateAsync(resourceFromRequest, resourceFromDatabase, cancellationToken);
        }

        /// <inheritdoc />
        public async Task DeleteAsync<TResource, TId>(TId id, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable<TId>
        {
            dynamic repository = GetWriteRepository(typeof(TResource));
            await repository.DeleteAsync(id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task SetRelationshipAsync<TResource>(TResource primaryResource, object secondaryResourceIds, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            dynamic repository = GetWriteRepository(typeof(TResource));
            await repository.SetRelationshipAsync(primaryResource, secondaryResourceIds, cancellationToken);
        }

        /// <inheritdoc />
        public async Task AddToToManyRelationshipAsync<TResource, TId>(TId primaryId, ISet<IIdentifiable> secondaryResourceIds, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable<TId>
        {
            dynamic repository = GetWriteRepository(typeof(TResource));
            await repository.AddToToManyRelationshipAsync(primaryId, secondaryResourceIds, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RemoveFromToManyRelationshipAsync<TResource>(TResource primaryResource, ISet<IIdentifiable> secondaryResourceIds, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            dynamic repository = GetWriteRepository(typeof(TResource));
            await repository.RemoveFromToManyRelationshipAsync(primaryResource, secondaryResourceIds, cancellationToken);
        }

        protected virtual object ResolveReadRepository(Type resourceType)
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

        private object GetWriteRepository(Type resourceType)
        {
            var writeRepository = ResolveWriteRepository(resourceType);

            if (_request.TransactionId != null)
            {
                if (!(writeRepository is IRepositorySupportsTransaction repository))
                {
                    var resourceContext = _resourceContextProvider.GetResourceContext(resourceType);
                    throw new MissingTransactionSupportException(resourceContext.PublicName);
                }

                if (repository.TransactionId != _request.TransactionId)
                {
                    throw new NonSharedTransactionException();
                }
            }

            return writeRepository;
        }

        protected virtual object ResolveWriteRepository(Type resourceType)
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
