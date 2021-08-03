using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests
{
    internal sealed class NeverResourceDefinitionAccessor : IResourceDefinitionAccessor
    {
        public IImmutableList<IncludeElementExpression> OnApplyIncludes(Type resourceType, IImmutableList<IncludeElementExpression> existingIncludes)
        {
            return existingIncludes;
        }

        public FilterExpression OnApplyFilter(Type resourceType, FilterExpression existingFilter)
        {
            return existingFilter;
        }

        public SortExpression OnApplySort(Type resourceType, SortExpression existingSort)
        {
            return existingSort;
        }

        public PaginationExpression OnApplyPagination(Type resourceType, PaginationExpression existingPagination)
        {
            return existingPagination;
        }

        public SparseFieldSetExpression OnApplySparseFieldSet(Type resourceType, SparseFieldSetExpression existingSparseFieldSet)
        {
            return existingSparseFieldSet;
        }

        public object GetQueryableHandlerForQueryStringParameter(Type resourceType, string parameterName)
        {
            return new QueryStringParameterHandlers<IIdentifiable>();
        }

        public IDictionary<string, object> GetMeta(Type resourceType, IIdentifiable resourceInstance)
        {
            return null;
        }

        public Task OnPrepareWriteAsync<TResource>(TResource resource, OperationKind operationKind, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            return Task.CompletedTask;
        }

        public Task<IIdentifiable> OnSetToOneRelationshipAsync<TResource>(TResource leftResource, HasOneAttribute hasOneRelationship,
            IIdentifiable rightResourceId, OperationKind operationKind, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            return Task.FromResult(rightResourceId);
        }

        public Task OnSetToManyRelationshipAsync<TResource>(TResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
            OperationKind operationKind, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            return Task.CompletedTask;
        }

        public Task OnAddToRelationshipAsync<TResource, TId>(TId leftResourceId, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
            CancellationToken cancellationToken)
            where TResource : class, IIdentifiable<TId>
        {
            return Task.CompletedTask;
        }

        public Task OnRemoveFromRelationshipAsync<TResource>(TResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
            CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            return Task.CompletedTask;
        }

        public Task OnWritingAsync<TResource>(TResource resource, OperationKind operationKind, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            return Task.CompletedTask;
        }

        public Task OnWriteSucceededAsync<TResource>(TResource resource, OperationKind operationKind, CancellationToken cancellationToken)
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
}
