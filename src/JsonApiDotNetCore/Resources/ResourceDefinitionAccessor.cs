using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Resources;

/// <inheritdoc cref="IResourceDefinitionAccessor" />
[PublicAPI]
public class ResourceDefinitionAccessor : IResourceDefinitionAccessor
{
    private readonly IResourceGraph _resourceGraph;
    private readonly IServiceProvider _serviceProvider;

    /// <inheritdoc />
    [Obsolete("Use IJsonApiRequest.IsReadOnly.")]
    public bool IsReadOnlyRequest
    {
        get
        {
            var request = _serviceProvider.GetRequiredService<IJsonApiRequest>();
            return request.IsReadOnly;
        }
    }

    /// <inheritdoc />
    [Obsolete("Use injected IQueryableBuilder instead.")]
    public IQueryableBuilder QueryableBuilder => _serviceProvider.GetRequiredService<IQueryableBuilder>();

    public ResourceDefinitionAccessor(IResourceGraph resourceGraph, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(resourceGraph);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _resourceGraph = resourceGraph;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public IImmutableSet<IncludeElementExpression> OnApplyIncludes(ResourceType resourceType, IImmutableSet<IncludeElementExpression> existingIncludes)
    {
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentNullException.ThrowIfNull(existingIncludes);

        object resourceDefinition = ResolveResourceDefinition(resourceType);

        Func<object, IImmutableSet<IncludeElementExpression>, IImmutableSet<IncludeElementExpression>> method =
            ResourceDefinitionAccessorCache.GetOnApplyIncludesDelegate(resourceDefinition.GetType());

        return method(resourceDefinition, existingIncludes);
    }

    /// <inheritdoc />
    public FilterExpression? OnApplyFilter(ResourceType resourceType, FilterExpression? existingFilter)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        object resourceDefinition = ResolveResourceDefinition(resourceType);

        Func<object, FilterExpression?, FilterExpression?> method = ResourceDefinitionAccessorCache.GetOnApplyFilterDelegate(resourceDefinition.GetType());

        return method(resourceDefinition, existingFilter);
    }

    /// <inheritdoc />
    public SortExpression? OnApplySort(ResourceType resourceType, SortExpression? existingSort)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        object resourceDefinition = ResolveResourceDefinition(resourceType);

        Func<object, SortExpression?, SortExpression?> method = ResourceDefinitionAccessorCache.GetOnApplySortDelegate(resourceDefinition.GetType());

        return method(resourceDefinition, existingSort);
    }

    /// <inheritdoc />
    public PaginationExpression? OnApplyPagination(ResourceType resourceType, PaginationExpression? existingPagination)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        object resourceDefinition = ResolveResourceDefinition(resourceType);

        Func<object, PaginationExpression?, PaginationExpression?> method =
            ResourceDefinitionAccessorCache.GetOnApplyPaginationDelegate(resourceDefinition.GetType());

        return method(resourceDefinition, existingPagination);
    }

    /// <inheritdoc />
    public SparseFieldSetExpression? OnApplySparseFieldSet(ResourceType resourceType, SparseFieldSetExpression? existingSparseFieldSet)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        object resourceDefinition = ResolveResourceDefinition(resourceType);

        Func<object, SparseFieldSetExpression?, SparseFieldSetExpression?> method =
            ResourceDefinitionAccessorCache.GetOnApplySparseFieldSetDelegate(resourceDefinition.GetType());

        return method(resourceDefinition, existingSparseFieldSet);
    }

    /// <inheritdoc />
    public object? GetQueryableHandlerForQueryStringParameter(Type resourceClrType, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(resourceClrType);
        ArgumentException.ThrowIfNullOrEmpty(parameterName);

        object resourceDefinition = ResolveResourceDefinition(resourceClrType);

        Func<object, string, object?> method = ResourceDefinitionAccessorCache.GetGetQueryableHandlerDelegate(resourceDefinition.GetType());

        return method(resourceDefinition, parameterName);
    }

    /// <inheritdoc />
    public IDictionary<string, object?>? GetMeta(ResourceType resourceType, IIdentifiable resourceInstance)
    {
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentNullException.ThrowIfNull(resourceInstance);

        object resourceDefinition = ResolveResourceDefinition(resourceType);

        Func<object, IIdentifiable, IDictionary<string, object?>?> method = ResourceDefinitionAccessorCache.GetGetMetaDelegate(resourceDefinition.GetType());

        return method(resourceDefinition, resourceInstance);
    }

    /// <inheritdoc />
    public async Task OnPrepareWriteAsync<TResource>(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(resource);

        object resourceDefinition = ResolveResourceDefinition(resource.GetClrType());

        Func<object, IIdentifiable, WriteOperationKind, CancellationToken, Task> method =
            ResourceDefinitionAccessorCache.GetOnPrepareWriteAsyncDelegate(resourceDefinition.GetType());

        await method(resourceDefinition, resource, writeOperation, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IIdentifiable?> OnSetToOneRelationshipAsync<TResource>(TResource leftResource, HasOneAttribute hasOneRelationship,
        IIdentifiable? rightResourceId, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(leftResource);
        ArgumentNullException.ThrowIfNull(hasOneRelationship);

        object resourceDefinition = ResolveResourceDefinition(leftResource.GetClrType());

        Func<object, IIdentifiable, HasOneAttribute, IIdentifiable?, WriteOperationKind, CancellationToken, Task<IIdentifiable?>> method =
            ResourceDefinitionAccessorCache.GetOnSetToOneRelationshipAsyncDelegate(resourceDefinition.GetType());

        return await method(resourceDefinition, leftResource, hasOneRelationship, rightResourceId, writeOperation, cancellationToken);
    }

    /// <inheritdoc />
    public async Task OnSetToManyRelationshipAsync<TResource>(TResource leftResource, HasManyAttribute hasManyRelationship,
        ISet<IIdentifiable> rightResourceIds, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(leftResource);
        ArgumentNullException.ThrowIfNull(hasManyRelationship);
        ArgumentNullException.ThrowIfNull(rightResourceIds);

        object resourceDefinition = ResolveResourceDefinition(leftResource.GetClrType());

        Func<object, IIdentifiable, HasManyAttribute, ISet<IIdentifiable>, WriteOperationKind, CancellationToken, Task> method =
            ResourceDefinitionAccessorCache.GetOnSetToManyRelationshipAsyncDelegate(resourceDefinition.GetType());

        await method(resourceDefinition, leftResource, hasManyRelationship, rightResourceIds, writeOperation, cancellationToken);
    }

    /// <inheritdoc />
    public async Task OnAddToRelationshipAsync<TResource>(TResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
        CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(leftResource);
        ArgumentNullException.ThrowIfNull(hasManyRelationship);
        ArgumentNullException.ThrowIfNull(rightResourceIds);

        object resourceDefinition = ResolveResourceDefinition(leftResource.GetClrType());

        Func<object, IIdentifiable, HasManyAttribute, ISet<IIdentifiable>, CancellationToken, Task> method =
            ResourceDefinitionAccessorCache.GetOnAddToRelationshipAsyncDelegate(resourceDefinition.GetType());

        await method(resourceDefinition, leftResource, hasManyRelationship, rightResourceIds, cancellationToken);
    }

    /// <inheritdoc />
    public async Task OnRemoveFromRelationshipAsync<TResource>(TResource leftResource, HasManyAttribute hasManyRelationship,
        ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(leftResource);
        ArgumentNullException.ThrowIfNull(hasManyRelationship);
        ArgumentNullException.ThrowIfNull(rightResourceIds);

        object resourceDefinition = ResolveResourceDefinition(leftResource.GetClrType());

        Func<object, IIdentifiable, HasManyAttribute, ISet<IIdentifiable>, CancellationToken, Task> method =
            ResourceDefinitionAccessorCache.GetOnRemoveFromRelationshipAsyncDelegate(resourceDefinition.GetType());

        await method(resourceDefinition, leftResource, hasManyRelationship, rightResourceIds, cancellationToken);
    }

    /// <inheritdoc />
    public async Task OnWritingAsync<TResource>(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(resource);

        object resourceDefinition = ResolveResourceDefinition(resource.GetClrType());

        Func<object, IIdentifiable, WriteOperationKind, CancellationToken, Task> method =
            ResourceDefinitionAccessorCache.GetOnWritingAsyncDelegate(resourceDefinition.GetType());

        await method(resourceDefinition, resource, writeOperation, cancellationToken);
    }

    /// <inheritdoc />
    public async Task OnWriteSucceededAsync<TResource>(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(resource);

        object resourceDefinition = ResolveResourceDefinition(resource.GetClrType());

        Func<object, IIdentifiable, WriteOperationKind, CancellationToken, Task> method =
            ResourceDefinitionAccessorCache.GetOnWriteSucceededAsyncDelegate(resourceDefinition.GetType());

        await method(resourceDefinition, resource, writeOperation, cancellationToken);
    }

    /// <inheritdoc />
    public void OnDeserialize(IIdentifiable resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        object resourceDefinition = ResolveResourceDefinition(resource.GetClrType());

        Action<object, IIdentifiable> method = ResourceDefinitionAccessorCache.GetOnDeserializeDelegate(resourceDefinition.GetType());

        method(resourceDefinition, resource);
    }

    /// <inheritdoc />
    public void OnSerialize(IIdentifiable resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        object resourceDefinition = ResolveResourceDefinition(resource.GetClrType());

        Action<object, IIdentifiable> method = ResourceDefinitionAccessorCache.GetOnSerializeDelegate(resourceDefinition.GetType());

        method(resourceDefinition, resource);
    }

    protected object ResolveResourceDefinition(Type resourceClrType)
    {
        ArgumentNullException.ThrowIfNull(resourceClrType);

        ResourceType resourceType = _resourceGraph.GetResourceType(resourceClrType);
        return ResolveResourceDefinition(resourceType);
    }

    protected virtual object ResolveResourceDefinition(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        Type resourceDefinitionType = ResourceDefinitionAccessorCache.GetResourceDefinitionType(resourceType);
        return _serviceProvider.GetRequiredService(resourceDefinitionType);
    }
}
