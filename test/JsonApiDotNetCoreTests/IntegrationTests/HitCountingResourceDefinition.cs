using System.Collections.Immutable;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests;

/// <summary>
/// Tracks invocations on <see cref="IResourceDefinition{TResource,TId}" /> callback methods. This is used solely in our tests, so we can assert which
/// calls were made, and in which order.
/// </summary>
public abstract class HitCountingResourceDefinition<TResource, TId> : JsonApiResourceDefinition<TResource, TId>
    where TResource : class, IIdentifiable<TId>
{
    private readonly ResourceDefinitionHitCounter _hitCounter;

    protected virtual ResourceDefinitionExtensibilityPoints ExtensibilityPointsToTrack => ResourceDefinitionExtensibilityPoints.All;

    protected HitCountingResourceDefinition(IResourceGraph resourceGraph, ResourceDefinitionHitCounter hitCounter)
        : base(resourceGraph)
    {
        ArgumentGuard.NotNull(hitCounter, nameof(hitCounter));

        _hitCounter = hitCounter;
    }

    public override IImmutableSet<IncludeElementExpression> OnApplyIncludes(IImmutableSet<IncludeElementExpression> existingIncludes)
    {
        if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoints.OnApplyIncludes))
        {
            _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoints.OnApplyIncludes);
        }

        return base.OnApplyIncludes(existingIncludes);
    }

    public override FilterExpression? OnApplyFilter(FilterExpression? existingFilter)
    {
        if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoints.OnApplyFilter))
        {
            _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoints.OnApplyFilter);
        }

        return base.OnApplyFilter(existingFilter);
    }

    public override SortExpression? OnApplySort(SortExpression? existingSort)
    {
        if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoints.OnApplySort))
        {
            _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoints.OnApplySort);
        }

        return base.OnApplySort(existingSort);
    }

    public override PaginationExpression? OnApplyPagination(PaginationExpression? existingPagination)
    {
        if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoints.OnApplyPagination))
        {
            _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoints.OnApplyPagination);
        }

        return base.OnApplyPagination(existingPagination);
    }

    public override SparseFieldSetExpression? OnApplySparseFieldSet(SparseFieldSetExpression? existingSparseFieldSet)
    {
        if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet))
        {
            _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet);
        }

        return base.OnApplySparseFieldSet(existingSparseFieldSet);
    }

    public override QueryStringParameterHandlers<TResource>? OnRegisterQueryableHandlersForQueryStringParameters()
    {
        if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoints.OnRegisterQueryableHandlersForQueryStringParameters))
        {
            _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoints.OnRegisterQueryableHandlersForQueryStringParameters);
        }

        return base.OnRegisterQueryableHandlersForQueryStringParameters();
    }

    public override IDictionary<string, object?>? GetMeta(TResource resource)
    {
        if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoints.GetMeta))
        {
            _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoints.GetMeta);
        }

        return base.GetMeta(resource);
    }

    public override Task OnPrepareWriteAsync(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoints.OnPrepareWriteAsync))
        {
            _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoints.OnPrepareWriteAsync);
        }

        return base.OnPrepareWriteAsync(resource, writeOperation, cancellationToken);
    }

    public override Task<IIdentifiable?> OnSetToOneRelationshipAsync(TResource leftResource, HasOneAttribute hasOneRelationship, IIdentifiable? rightResourceId,
        WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoints.OnSetToOneRelationshipAsync))
        {
            _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoints.OnSetToOneRelationshipAsync);
        }

        return base.OnSetToOneRelationshipAsync(leftResource, hasOneRelationship, rightResourceId, writeOperation, cancellationToken);
    }

    public override Task OnSetToManyRelationshipAsync(TResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
        WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoints.OnSetToManyRelationshipAsync))
        {
            _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoints.OnSetToManyRelationshipAsync);
        }

        return base.OnSetToManyRelationshipAsync(leftResource, hasManyRelationship, rightResourceIds, writeOperation, cancellationToken);
    }

    public override Task OnAddToRelationshipAsync(TResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
        CancellationToken cancellationToken)
    {
        if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoints.OnAddToRelationshipAsync))
        {
            _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoints.OnAddToRelationshipAsync);
        }

        return base.OnAddToRelationshipAsync(leftResource, hasManyRelationship, rightResourceIds, cancellationToken);
    }

    public override Task OnRemoveFromRelationshipAsync(TResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
        CancellationToken cancellationToken)
    {
        if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoints.OnRemoveFromRelationshipAsync))
        {
            _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoints.OnRemoveFromRelationshipAsync);
        }

        return base.OnRemoveFromRelationshipAsync(leftResource, hasManyRelationship, rightResourceIds, cancellationToken);
    }

    public override Task OnWritingAsync(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoints.OnWritingAsync))
        {
            _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoints.OnWritingAsync);
        }

        return base.OnWritingAsync(resource, writeOperation, cancellationToken);
    }

    public override Task OnWriteSucceededAsync(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoints.OnWriteSucceededAsync))
        {
            _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoints.OnWriteSucceededAsync);
        }

        return base.OnWriteSucceededAsync(resource, writeOperation, cancellationToken);
    }

    public override void OnDeserialize(TResource resource)
    {
        if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoints.OnDeserialize))
        {
            _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoints.OnDeserialize);
        }

        base.OnDeserialize(resource);
    }

    public override void OnSerialize(TResource resource)
    {
        if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoints.OnSerialize))
        {
            _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoints.OnSerialize);
        }

        base.OnSerialize(resource);
    }
}
