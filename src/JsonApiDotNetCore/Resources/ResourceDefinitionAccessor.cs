using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Resources;

/// <inheritdoc />
[PublicAPI]
public class ResourceDefinitionAccessor : IResourceDefinitionAccessor
{
    private readonly IResourceGraph _resourceGraph;
    private readonly IServiceProvider _serviceProvider;

    public ResourceDefinitionAccessor(IResourceGraph resourceGraph, IServiceProvider serviceProvider)
    {
        ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));
        ArgumentGuard.NotNull(serviceProvider, nameof(serviceProvider));

        _resourceGraph = resourceGraph;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public IImmutableSet<IncludeElementExpression> OnApplyIncludes(ResourceType resourceType, IImmutableSet<IncludeElementExpression> existingIncludes)
    {
        ArgumentGuard.NotNull(resourceType, nameof(resourceType));

        dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
        return resourceDefinition.OnApplyIncludes(existingIncludes);
    }

    /// <inheritdoc />
    public FilterExpression? OnApplyFilter(ResourceType resourceType, FilterExpression? existingFilter)
    {
        ArgumentGuard.NotNull(resourceType, nameof(resourceType));

        dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
        return resourceDefinition.OnApplyFilter(existingFilter);
    }

    /// <inheritdoc />
    public SortExpression? OnApplySort(ResourceType resourceType, SortExpression? existingSort)
    {
        ArgumentGuard.NotNull(resourceType, nameof(resourceType));

        dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
        return resourceDefinition.OnApplySort(existingSort);
    }

    /// <inheritdoc />
    public PaginationExpression? OnApplyPagination(ResourceType resourceType, PaginationExpression? existingPagination)
    {
        ArgumentGuard.NotNull(resourceType, nameof(resourceType));

        dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
        return resourceDefinition.OnApplyPagination(existingPagination);
    }

    /// <inheritdoc />
    public SparseFieldSetExpression? OnApplySparseFieldSet(ResourceType resourceType, SparseFieldSetExpression? existingSparseFieldSet)
    {
        ArgumentGuard.NotNull(resourceType, nameof(resourceType));

        dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
        return resourceDefinition.OnApplySparseFieldSet(existingSparseFieldSet);
    }

    /// <inheritdoc />
    public object? GetQueryableHandlerForQueryStringParameter(Type resourceClrType, string parameterName)
    {
        ArgumentGuard.NotNull(resourceClrType, nameof(resourceClrType));
        ArgumentGuard.NotNullNorEmpty(parameterName, nameof(parameterName));

        dynamic resourceDefinition = ResolveResourceDefinition(resourceClrType);
        dynamic handlers = resourceDefinition.OnRegisterQueryableHandlersForQueryStringParameters();

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
        ArgumentGuard.NotNull(resourceType, nameof(resourceType));

        dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
        return resourceDefinition.GetMeta((dynamic)resourceInstance);
    }

    /// <inheritdoc />
    public async Task OnPrepareWriteAsync<TResource>(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentGuard.NotNull(resource, nameof(resource));

        dynamic resourceDefinition = ResolveResourceDefinition(resource.GetClrType());
        await resourceDefinition.OnPrepareWriteAsync((dynamic)resource, writeOperation, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IIdentifiable?> OnSetToOneRelationshipAsync<TResource>(TResource leftResource, HasOneAttribute hasOneRelationship,
        IIdentifiable? rightResourceId, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentGuard.NotNull(leftResource, nameof(leftResource));
        ArgumentGuard.NotNull(hasOneRelationship, nameof(hasOneRelationship));

        dynamic resourceDefinition = ResolveResourceDefinition(leftResource.GetClrType());

        return await resourceDefinition.OnSetToOneRelationshipAsync((dynamic)leftResource, hasOneRelationship, rightResourceId, writeOperation,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task OnSetToManyRelationshipAsync<TResource>(TResource leftResource, HasManyAttribute hasManyRelationship,
        ISet<IIdentifiable> rightResourceIds, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentGuard.NotNull(leftResource, nameof(leftResource));
        ArgumentGuard.NotNull(hasManyRelationship, nameof(hasManyRelationship));
        ArgumentGuard.NotNull(rightResourceIds, nameof(rightResourceIds));

        dynamic resourceDefinition = ResolveResourceDefinition(leftResource.GetClrType());
        await resourceDefinition.OnSetToManyRelationshipAsync((dynamic)leftResource, hasManyRelationship, rightResourceIds, writeOperation, cancellationToken);
    }

    /// <inheritdoc />
    public async Task OnAddToRelationshipAsync<TResource>(TResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
        CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentGuard.NotNull(hasManyRelationship, nameof(hasManyRelationship));
        ArgumentGuard.NotNull(rightResourceIds, nameof(rightResourceIds));

        dynamic resourceDefinition = ResolveResourceDefinition(leftResource.GetClrType());
        await resourceDefinition.OnAddToRelationshipAsync((dynamic)leftResource, hasManyRelationship, rightResourceIds, cancellationToken);
    }

    /// <inheritdoc />
    public async Task OnRemoveFromRelationshipAsync<TResource>(TResource leftResource, HasManyAttribute hasManyRelationship,
        ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentGuard.NotNull(leftResource, nameof(leftResource));
        ArgumentGuard.NotNull(hasManyRelationship, nameof(hasManyRelationship));
        ArgumentGuard.NotNull(rightResourceIds, nameof(rightResourceIds));

        dynamic resourceDefinition = ResolveResourceDefinition(leftResource.GetClrType());
        await resourceDefinition.OnRemoveFromRelationshipAsync((dynamic)leftResource, hasManyRelationship, rightResourceIds, cancellationToken);
    }

    /// <inheritdoc />
    public async Task OnWritingAsync<TResource>(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentGuard.NotNull(resource, nameof(resource));

        dynamic resourceDefinition = ResolveResourceDefinition(resource.GetClrType());
        await resourceDefinition.OnWritingAsync((dynamic)resource, writeOperation, cancellationToken);
    }

    /// <inheritdoc />
    public async Task OnWriteSucceededAsync<TResource>(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        ArgumentGuard.NotNull(resource, nameof(resource));

        dynamic resourceDefinition = ResolveResourceDefinition(resource.GetClrType());
        await resourceDefinition.OnWriteSucceededAsync((dynamic)resource, writeOperation, cancellationToken);
    }

    /// <inheritdoc />
    public void OnDeserialize(IIdentifiable resource)
    {
        ArgumentGuard.NotNull(resource, nameof(resource));

        dynamic resourceDefinition = ResolveResourceDefinition(resource.GetClrType());
        resourceDefinition.OnDeserialize((dynamic)resource);
    }

    /// <inheritdoc />
    public void OnSerialize(IIdentifiable resource)
    {
        ArgumentGuard.NotNull(resource, nameof(resource));

        dynamic resourceDefinition = ResolveResourceDefinition(resource.GetClrType());
        resourceDefinition.OnSerialize((dynamic)resource);
    }

    protected object ResolveResourceDefinition(Type resourceClrType)
    {
        ResourceType resourceType = _resourceGraph.GetResourceType(resourceClrType);
        return ResolveResourceDefinition(resourceType);
    }

    protected virtual object ResolveResourceDefinition(ResourceType resourceType)
    {
        Type resourceDefinitionType = typeof(IResourceDefinition<,>).MakeGenericType(resourceType.ClrType, resourceType.IdentityClrType);
        return _serviceProvider.GetRequiredService(resourceDefinitionType);
    }
}
