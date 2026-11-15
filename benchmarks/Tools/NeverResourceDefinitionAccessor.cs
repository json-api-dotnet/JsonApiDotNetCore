using System.Collections.Immutable;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Benchmarks.Tools;

/// <summary>
/// Never calls into <see cref="IResourceDefinition{TResource,TId}" /> instances.
/// </summary>
internal sealed class NeverResourceDefinitionAccessor : IResourceDefinitionAccessor
{
    public IImmutableSet<IncludeElementExpression> OnApplyIncludes(ResourceType resourceType, IImmutableSet<IncludeElementExpression> existingIncludes)
    {
        return existingIncludes;
    }

    public FilterExpression? OnApplyFilter(ResourceType resourceType, FilterExpression? existingFilter)
    {
        return existingFilter;
    }

    public SortExpression? OnApplySort(ResourceType resourceType, SortExpression? existingSort)
    {
        return existingSort;
    }

    public PaginationExpression? OnApplyPagination(ResourceType resourceType, PaginationExpression? existingPagination)
    {
        return existingPagination;
    }

    public SparseFieldSetExpression? OnApplySparseFieldSet(ResourceType resourceType, SparseFieldSetExpression? existingSparseFieldSet)
    {
        return existingSparseFieldSet;
    }

    public object? GetQueryableHandlerForQueryStringParameter(Type resourceClrType, string parameterName)
    {
        return null;
    }

    public IDictionary<string, object?>? GetMeta(ResourceType resourceType, IIdentifiable resourceInstance)
    {
        return null;
    }

    public Task OnPrepareWriteAsync<TResource>(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        return Task.CompletedTask;
    }

    public Task<IIdentifiable?> OnSetToOneRelationshipAsync<TResource>(TResource leftResource, HasOneAttribute hasOneRelationship,
        IIdentifiable? rightResourceId, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        return Task.FromResult(rightResourceId);
    }

    public Task OnSetToManyRelationshipAsync<TResource>(TResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
        WriteOperationKind writeOperation, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        return Task.CompletedTask;
    }

    public Task OnAddToRelationshipAsync<TResource>(TResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
        CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        return Task.CompletedTask;
    }

    public Task OnRemoveFromRelationshipAsync<TResource>(TResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
        CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        return Task.CompletedTask;
    }

    public Task OnWritingAsync<TResource>(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        return Task.CompletedTask;
    }

    public Task OnWriteSucceededAsync<TResource>(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable
    {
        return Task.CompletedTask;
    }

    public void OnDeserialize(IIdentifiable resource)
    {
    }

    public void OnSerialize(IIdentifiable resource)
    {
    }
}
