using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Repositories;

/// <inheritdoc cref="IResourceRepositoryAccessor" />
[PublicAPI]
public class ResourceRepositoryAccessor : IResourceRepositoryAccessor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IResourceGraph _resourceGraph;
    private readonly IJsonApiRequest _request;

    public ResourceRepositoryAccessor(IServiceProvider serviceProvider, IResourceGraph resourceGraph, IJsonApiRequest request)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(resourceGraph);
        ArgumentNullException.ThrowIfNull(request);

        _serviceProvider = serviceProvider;
        _resourceGraph = resourceGraph;
        _request = request;
    }

    /// <inheritdoc />
    public ResourceType LookupResourceType(Type resourceClrType)
    {
        ArgumentNullException.ThrowIfNull(resourceClrType);

        return _resourceGraph.GetResourceType(resourceClrType);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<TResource>> GetAsync<TResource>(QueryLayer queryLayer, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(queryLayer);

        object repository = ResolveReadRepository(typeof(TResource));

        Func<object, QueryLayer, CancellationToken, Task<IReadOnlyCollection<TResource>>> method =
            RepositoryAccessorCache<TResource>.GetGetAsyncDelegate(repository.GetType());

        return await method(repository, queryLayer, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<IIdentifiable>> GetAsync(ResourceType resourceType, QueryLayer queryLayer, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentNullException.ThrowIfNull(queryLayer);

        object repository = ResolveReadRepository(resourceType);

        Func<object, QueryLayer, CancellationToken, Task<IReadOnlyCollection<IIdentifiable>>> method =
            ResourceRepositoryAccessorCache.GetGetAsyncDelegate(repository.GetType());

        return await method(repository, queryLayer, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(ResourceType resourceType, FilterExpression? filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        object repository = ResolveReadRepository(resourceType);

        Func<object, FilterExpression?, CancellationToken, Task<int>> method = ResourceRepositoryAccessorCache.GetCountAsyncDelegate(repository.GetType());

        return await method(repository, filter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TResource> GetForCreateAsync<TResource, TId>(Type resourceClrType, TId id, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable<TId>
    {
        ArgumentNullException.ThrowIfNull(resourceClrType);

        object repository = GetWriteRepository(typeof(TResource));

        Func<object, Type, TId, CancellationToken, Task<TResource>> method =
            RepositoryAccessorCache<TResource, TId>.GetGetForCreateAsyncDelegate(repository.GetType());

        return await method(repository, resourceClrType, id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task CreateAsync<TResource>(TResource resourceFromRequest, TResource resourceForDatabase, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(resourceFromRequest);
        ArgumentNullException.ThrowIfNull(resourceForDatabase);

        object repository = GetWriteRepository(typeof(TResource));

        Func<object, TResource, TResource, CancellationToken, Task> method = RepositoryAccessorCache<TResource>.GetCreateAsyncDelegate(repository.GetType());

        await method(repository, resourceFromRequest, resourceForDatabase, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TResource?> GetForUpdateAsync<TResource>(QueryLayer queryLayer, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(queryLayer);

        object repository = GetWriteRepository(typeof(TResource));

        Func<object, QueryLayer, CancellationToken, Task<TResource?>> method =
            RepositoryAccessorCache<TResource>.GetGetForUpdateAsyncDelegate(repository.GetType());

        return await method(repository, queryLayer, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync<TResource>(TResource resourceFromRequest, TResource resourceFromDatabase, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(resourceFromRequest);
        ArgumentNullException.ThrowIfNull(resourceFromDatabase);

        object repository = GetWriteRepository(typeof(TResource));

        Func<object, TResource, TResource, CancellationToken, Task> method = RepositoryAccessorCache<TResource>.GetUpdateAsyncDelegate(repository.GetType());

        await method(repository, resourceFromRequest, resourceFromDatabase, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync<TResource, TId>(TResource? resourceFromDatabase, [DisallowNull] TId id, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable<TId>
    {
        ArgumentNullException.ThrowIfNull(id);

        object repository = GetWriteRepository(typeof(TResource));

        Func<object, TResource?, TId, CancellationToken, Task> method = RepositoryAccessorCache<TResource, TId>.GetDeleteAsyncDelegate(repository.GetType());

        await method(repository, resourceFromDatabase, id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SetRelationshipAsync<TResource>(TResource leftResource, object? rightValue, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(leftResource);

        object repository = GetWriteRepository(typeof(TResource));

        Func<object, TResource, object?, CancellationToken, Task> method =
            RepositoryAccessorCache<TResource>.GetSetRelationshipAsyncDelegate(repository.GetType());

        await method(repository, leftResource, rightValue, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddToToManyRelationshipAsync<TResource, TId>(TResource? leftResource, [DisallowNull] TId leftId, ISet<IIdentifiable> rightResourceIds,
        CancellationToken cancellationToken)
        where TResource : class, IIdentifiable<TId>
    {
        ArgumentNullException.ThrowIfNull(leftId);
        ArgumentNullException.ThrowIfNull(rightResourceIds);

        object repository = GetWriteRepository(typeof(TResource));

        Func<object, TResource?, TId, ISet<IIdentifiable>, CancellationToken, Task> method =
            RepositoryAccessorCache<TResource, TId>.GetAddToToManyRelationshipAsyncDelegate(repository.GetType());

        await method(repository, leftResource, leftId, rightResourceIds, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveFromToManyRelationshipAsync<TResource>(TResource leftResource, ISet<IIdentifiable> rightResourceIds,
        CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(leftResource);
        ArgumentNullException.ThrowIfNull(rightResourceIds);

        object repository = GetWriteRepository(typeof(TResource));

        Func<object, TResource, ISet<IIdentifiable>, CancellationToken, Task> method =
            RepositoryAccessorCache<TResource>.GetRemoveFromToManyRelationshipAsyncDelegate(repository.GetType());

        await method(repository, leftResource, rightResourceIds, cancellationToken);
    }

    protected object ResolveReadRepository(Type resourceClrType)
    {
        ArgumentNullException.ThrowIfNull(resourceClrType);

        ResourceType resourceType = _resourceGraph.GetResourceType(resourceClrType);
        return ResolveReadRepository(resourceType);
    }

    protected virtual object ResolveReadRepository(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        Type repositoryType = ResourceRepositoryAccessorCache.GetReadRepositoryType(resourceType);
        return _serviceProvider.GetRequiredService(repositoryType);
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
        ArgumentNullException.ThrowIfNull(resourceClrType);

        ResourceType resourceType = _resourceGraph.GetResourceType(resourceClrType);

        Type repositoryType = ResourceRepositoryAccessorCache.GetWriteRepositoryType(resourceType);
        return _serviceProvider.GetRequiredService(repositoryType);
    }
}
