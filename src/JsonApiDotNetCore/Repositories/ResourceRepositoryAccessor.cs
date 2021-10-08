using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
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
    [PublicAPI]
    public class ResourceRepositoryAccessor : IResourceRepositoryAccessor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IResourceGraph _resourceGraph;
        private readonly IJsonApiRequest _request;

        public ResourceRepositoryAccessor(IServiceProvider serviceProvider, IResourceGraph resourceGraph, IJsonApiRequest request)
        {
            ArgumentGuard.NotNull(serviceProvider, nameof(serviceProvider));
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));
            ArgumentGuard.NotNull(request, nameof(request));

            _serviceProvider = serviceProvider;
            _resourceGraph = resourceGraph;
            _request = request;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<TResource>> GetAsync<TResource>(QueryLayer layer, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            dynamic repository = ResolveReadRepository(typeof(TResource));
            return (IReadOnlyCollection<TResource>)await repository.GetAsync(layer, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IIdentifiable>> GetAsync(ResourceType resourceType, QueryLayer layer, CancellationToken cancellationToken)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            dynamic repository = ResolveReadRepository(resourceType);
            return (IReadOnlyCollection<IIdentifiable>)await repository.GetAsync(layer, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<int> CountAsync<TResource>(FilterExpression topFilter, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            dynamic repository = ResolveReadRepository(typeof(TResource));
            return (int)await repository.CountAsync(topFilter, cancellationToken);
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
        public async Task SetRelationshipAsync<TResource>(TResource leftResource, object rightValue, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            dynamic repository = GetWriteRepository(typeof(TResource));
            await repository.SetRelationshipAsync(leftResource, rightValue, cancellationToken);
        }

        /// <inheritdoc />
        public async Task AddToToManyRelationshipAsync<TResource, TId>(TId leftId, ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable<TId>
        {
            dynamic repository = GetWriteRepository(typeof(TResource));
            await repository.AddToToManyRelationshipAsync(leftId, rightResourceIds, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RemoveFromToManyRelationshipAsync<TResource>(TResource leftResource, ISet<IIdentifiable> rightResourceIds,
            CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            dynamic repository = GetWriteRepository(typeof(TResource));
            await repository.RemoveFromToManyRelationshipAsync(leftResource, rightResourceIds, cancellationToken);
        }

        protected object ResolveReadRepository(Type resourceClrType)
        {
            ResourceType resourceType = _resourceGraph.GetResourceType(resourceClrType);
            return ResolveReadRepository(resourceType);
        }

        protected virtual object ResolveReadRepository(ResourceType resourceType)
        {
            Type resourceDefinitionType = typeof(IResourceReadRepository<,>).MakeGenericType(resourceType.ClrType, resourceType.IdentityClrType);
            return _serviceProvider.GetRequiredService(resourceDefinitionType);
        }

        private object GetWriteRepository(Type resourceClrType)
        {
            object writeRepository = ResolveWriteRepository(resourceClrType);

            if (_request.TransactionId != null)
            {
                if (writeRepository is not IRepositorySupportsTransaction repository)
                {
                    ResourceType resourceType = _resourceGraph.GetResourceType(resourceClrType);
                    throw new MissingTransactionSupportException(resourceType.PublicName);
                }

                if (repository.TransactionId != _request.TransactionId)
                {
                    throw new NonParticipatingTransactionException();
                }
            }

            return writeRepository;
        }

        protected virtual object ResolveWriteRepository(Type resourceClrType)
        {
            ResourceType resourceType = _resourceGraph.GetResourceType(resourceClrType);

            Type resourceDefinitionType = typeof(IResourceWriteRepository<,>).MakeGenericType(resourceType.ClrType, resourceType.IdentityClrType);
            return _serviceProvider.GetRequiredService(resourceDefinitionType);
        }
    }
}
