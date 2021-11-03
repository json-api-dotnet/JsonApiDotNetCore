using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests
{
    /// <summary>
    /// Tracks invocations on <see cref="IResourceDefinition{TResource,TId}" /> callback methods. This is used solely in our tests, so we can assert which
    /// calls were made, and in which order.
    /// </summary>
    public abstract class HitCountingResourceDefinition<TResource, TId> : JsonApiResourceDefinition<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly ResourceDefinitionHitCounter _hitCounter;

        protected virtual ResourceDefinitionExtensibilityPoint ExtensibilityPointsToTrack => ResourceDefinitionExtensibilityPoint.All;

        protected HitCountingResourceDefinition(IResourceGraph resourceGraph, ResourceDefinitionHitCounter hitCounter)
            : base(resourceGraph)
        {
            ArgumentGuard.NotNull(hitCounter, nameof(hitCounter));

            _hitCounter = hitCounter;
        }

        public override IImmutableSet<IncludeElementExpression> OnApplyIncludes(IImmutableSet<IncludeElementExpression> existingIncludes)
        {
            if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoint.OnApplyIncludes))
            {
                _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoint.OnApplyIncludes);
            }

            return base.OnApplyIncludes(existingIncludes);
        }

        public override FilterExpression? OnApplyFilter(FilterExpression? existingFilter)
        {
            if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoint.OnApplyFilter))
            {
                _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoint.OnApplyFilter);
            }

            return base.OnApplyFilter(existingFilter);
        }

        public override SortExpression? OnApplySort(SortExpression? existingSort)
        {
            if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoint.OnApplySort))
            {
                _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoint.OnApplySort);
            }

            return base.OnApplySort(existingSort);
        }

        public override PaginationExpression? OnApplyPagination(PaginationExpression? existingPagination)
        {
            if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoint.OnApplyPagination))
            {
                _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoint.OnApplyPagination);
            }

            return base.OnApplyPagination(existingPagination);
        }

        public override SparseFieldSetExpression? OnApplySparseFieldSet(SparseFieldSetExpression? existingSparseFieldSet)
        {
            if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoint.OnApplySparseFieldSet))
            {
                _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoint.OnApplySparseFieldSet);
            }

            return base.OnApplySparseFieldSet(existingSparseFieldSet);
        }

        public override QueryStringParameterHandlers<TResource>? OnRegisterQueryableHandlersForQueryStringParameters()
        {
            if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoint.OnRegisterQueryableHandlersForQueryStringParameters))
            {
                _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoint.OnRegisterQueryableHandlersForQueryStringParameters);
            }

            return base.OnRegisterQueryableHandlersForQueryStringParameters();
        }

        public override IDictionary<string, object?>? GetMeta(TResource resource)
        {
            if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoint.GetMeta))
            {
                _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoint.GetMeta);
            }

            return base.GetMeta(resource);
        }

        public override Task OnPrepareWriteAsync(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoint.OnPrepareWriteAsync))
            {
                _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoint.OnPrepareWriteAsync);
            }

            return base.OnPrepareWriteAsync(resource, writeOperation, cancellationToken);
        }

        public override Task<IIdentifiable?> OnSetToOneRelationshipAsync(TResource leftResource, HasOneAttribute hasOneRelationship,
            IIdentifiable? rightResourceId, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoint.OnSetToOneRelationshipAsync))
            {
                _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoint.OnSetToOneRelationshipAsync);
            }

            return base.OnSetToOneRelationshipAsync(leftResource, hasOneRelationship, rightResourceId, writeOperation, cancellationToken);
        }

        public override Task OnSetToManyRelationshipAsync(TResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
            WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoint.OnSetToManyRelationshipAsync))
            {
                _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoint.OnSetToManyRelationshipAsync);
            }

            return base.OnSetToManyRelationshipAsync(leftResource, hasManyRelationship, rightResourceIds, writeOperation, cancellationToken);
        }

        public override Task OnAddToRelationshipAsync(TId leftResourceId, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
            CancellationToken cancellationToken)
        {
            if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoint.OnAddToRelationshipAsync))
            {
                _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoint.OnAddToRelationshipAsync);
            }

            return base.OnAddToRelationshipAsync(leftResourceId, hasManyRelationship, rightResourceIds, cancellationToken);
        }

        public override Task OnRemoveFromRelationshipAsync(TResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
            CancellationToken cancellationToken)
        {
            if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoint.OnRemoveFromRelationshipAsync))
            {
                _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoint.OnRemoveFromRelationshipAsync);
            }

            return base.OnRemoveFromRelationshipAsync(leftResource, hasManyRelationship, rightResourceIds, cancellationToken);
        }

        public override Task OnWritingAsync(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoint.OnWritingAsync))
            {
                _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoint.OnWritingAsync);
            }

            return base.OnWritingAsync(resource, writeOperation, cancellationToken);
        }

        public override Task OnWriteSucceededAsync(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoint.OnWriteSucceededAsync))
            {
                _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoint.OnWriteSucceededAsync);
            }

            return base.OnWriteSucceededAsync(resource, writeOperation, cancellationToken);
        }

        public override void OnDeserialize(TResource resource)
        {
            if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoint.OnDeserialize))
            {
                _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoint.OnDeserialize);
            }

            base.OnDeserialize(resource);
        }

        public override void OnSerialize(TResource resource)
        {
            if (ExtensibilityPointsToTrack.HasFlag(ResourceDefinitionExtensibilityPoint.OnSerialize))
            {
                _hitCounter.TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoint.OnSerialize);
            }

            base.OnSerialize(resource);
        }
    }
}
