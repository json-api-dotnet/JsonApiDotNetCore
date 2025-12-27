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
    public bool UseTrackingBehaviorHack
    {
        get
        {
            var options = _serviceProvider.GetRequiredService<IJsonApiOptions>();
            return options.UseTrackingBehaviorHack;

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

        dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
        return resourceDefinition.OnApplyIncludes(existingIncludes);
    }

    /// <inheritdoc />
    public FilterExpression? OnApplyFilter(ResourceType resourceType, FilterExpression? existingFilter)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
        return resourceDefinition.OnApplyFilter(existingFilter);
    }

    /// <inheritdoc />
    public SortExpression? OnApplySort(ResourceType resourceType, SortExpression? existingSort)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
        return resourceDefinition.OnApplySort(existingSort);
    }

    /// <inheritdoc />
    public PaginationExpression? OnApplyPagination(ResourceType resourceType, PaginationExpression? existingPagination)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
        return resourceDefinition.OnApplyPagination(existingPagination);
    }

    /// <inheritdoc />
    public SparseFieldSetExpression? OnApplySparseFieldSet(ResourceType resourceType, SparseFieldSetExpression? existingSparseFieldSet)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
        return resourceDefinition.OnApplySparseFieldSet(existingSparseFieldSet);
    }

    /// <inheritdoc />
    public object? GetQueryableHandlerForQueryStringParameter(Type resourceClrType, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(resourceClrType);
        ArgumentException.ThrowIfNullOrEmpty(parameterName);

        dynamic resourceDefinition = ResolveResourceDefinition(resourceClrType);
        dynamic? handlers = resourceDefinition.OnRegisterQueryableHandlersForQueryStringParameters();

        if (handlers != null)
        {
            if (handlers.ContainsKey(parameterName))
            {
                return handlers[parameterName];
            }
        }

        return null;
    }

    /// <inheritdoc />
    public IDictionary<string, object?>? GetMeta(ResourceType resourceType, IIdentifiable resourceInstance)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
        return resourceDefinition.GetMeta((dynamic)resourceInstance);
    }

    /// <inheritdoc />
    public async Task OnPrepareWriteAsync<TResource>(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(resource);

        dynamic resourceDefinition = ResolveResourceDefinition(resource.GetClrType());
        await resourceDefinition.OnPrepareWriteAsync((dynamic)resource, writeOperation, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IIdentifiable?> OnSetToOneRelationshipAsync<TResource>(TResource leftResource, HasOneAttribute hasOneRelationship,
        IIdentifiable? rightResourceId, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(leftResource);
        ArgumentNullException.ThrowIfNull(hasOneRelationship);

        dynamic resourceDefinition = ResolveResourceDefinition(leftResource.GetClrType());

        return await resourceDefinition.OnSetToOneRelationshipAsync((dynamic)leftResource, hasOneRelationship, rightResourceId, writeOperation,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task OnSetToManyRelationshipAsync<TResource>(TResource leftResource, HasManyAttribute hasManyRelationship,
        ISet<IIdentifiable> rightResourceIds, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(leftResource);
        ArgumentNullException.ThrowIfNull(hasManyRelationship);
        ArgumentNullException.ThrowIfNull(rightResourceIds);

        dynamic resourceDefinition = ResolveResourceDefinition(leftResource.GetClrType());
        await resourceDefinition.OnSetToManyRelationshipAsync((dynamic)leftResource, hasManyRelationship, rightResourceIds, writeOperation, cancellationToken);
    }

    /// <inheritdoc />
    public async Task OnAddToRelationshipAsync<TResource>(TResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
        CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(hasManyRelationship);
        ArgumentNullException.ThrowIfNull(rightResourceIds);

        dynamic resourceDefinition = ResolveResourceDefinition(leftResource.GetClrType());
        await resourceDefinition.OnAddToRelationshipAsync((dynamic)leftResource, hasManyRelationship, rightResourceIds, cancellationToken);
    }

    /// <inheritdoc />
    public async Task OnRemoveFromRelationshipAsync<TResource>(TResource leftResource, HasManyAttribute hasManyRelationship,
        ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(leftResource);
        ArgumentNullException.ThrowIfNull(hasManyRelationship);
        ArgumentNullException.ThrowIfNull(rightResourceIds);

        dynamic resourceDefinition = ResolveResourceDefinition(leftResource.GetClrType());
        await resourceDefinition.OnRemoveFromRelationshipAsync((dynamic)leftResource, hasManyRelationship, rightResourceIds, cancellationToken);
    }

    /// <inheritdoc />
    public async Task OnWritingAsync<TResource>(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(resource);

        dynamic resourceDefinition = ResolveResourceDefinition(resource.GetClrType());
        await resourceDefinition.OnWritingAsync((dynamic)resource, writeOperation, cancellationToken);
    }

    /// <inheritdoc />
    public async Task OnWriteSucceededAsync<TResource>(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(resource);

        dynamic resourceDefinition = ResolveResourceDefinition(resource.GetClrType());
        await resourceDefinition.OnWriteSucceededAsync((dynamic)resource, writeOperation, cancellationToken);
    }

    /// <inheritdoc />
    public void OnDeserialize(IIdentifiable resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        dynamic resourceDefinition = ResolveResourceDefinition(resource.GetClrType());
        resourceDefinition.OnDeserialize((dynamic)resource);
    }

    /// <inheritdoc />
    public void OnSerialize(IIdentifiable resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        dynamic resourceDefinition = ResolveResourceDefinition(resource.GetClrType());
        resourceDefinition.OnSerialize((dynamic)resource);
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

        Type resourceDefinitionType = typeof(IResourceDefinition<,>).MakeGenericType(resourceType.ClrType, resourceType.IdentityClrType);
        return _serviceProvider.GetRequiredService(resourceDefinitionType);
    }
}
