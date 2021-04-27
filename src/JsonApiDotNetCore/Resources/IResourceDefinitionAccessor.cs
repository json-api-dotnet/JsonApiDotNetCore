using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Resources
{
    /// <summary>
    /// Retrieves an <see cref="IResourceDefinition{TResource,TId}" /> instance from the D/I container and invokes a callback on it.
    /// </summary>
    public interface IResourceDefinitionAccessor
    {
        /// <summary>
        /// Invokes <see cref="IResourceDefinition{TResource,TId}.OnApplyIncludes" /> for the specified resource type.
        /// </summary>
        IReadOnlyCollection<IncludeElementExpression> OnApplyIncludes(Type resourceType, IReadOnlyCollection<IncludeElementExpression> existingIncludes);

        /// <summary>
        /// Invokes <see cref="IResourceDefinition{TResource,TId}.OnApplyFilter" /> for the specified resource type.
        /// </summary>
        FilterExpression OnApplyFilter(Type resourceType, FilterExpression existingFilter);

        /// <summary>
        /// Invokes <see cref="IResourceDefinition{TResource,TId}.OnApplySort" /> for the specified resource type.
        /// </summary>
        SortExpression OnApplySort(Type resourceType, SortExpression existingSort);

        /// <summary>
        /// Invokes <see cref="IResourceDefinition{TResource,TId}.OnApplyPagination" /> for the specified resource type.
        /// </summary>
        PaginationExpression OnApplyPagination(Type resourceType, PaginationExpression existingPagination);

        /// <summary>
        /// Invokes <see cref="IResourceDefinition{TResource,TId}.OnApplySparseFieldSet" /> for the specified resource type.
        /// </summary>
        SparseFieldSetExpression OnApplySparseFieldSet(Type resourceType, SparseFieldSetExpression existingSparseFieldSet);

        /// <summary>
        /// Invokes <see cref="IResourceDefinition{TResource,TId}.OnRegisterQueryableHandlersForQueryStringParameters" /> for the specified resource type, then
        /// returns the <see cref="IQueryable{T}" /> expression for the specified parameter name.
        /// </summary>
        object GetQueryableHandlerForQueryStringParameter(Type resourceType, string parameterName);

        /// <summary>
        /// Invokes <see cref="IResourceDefinition{TResource,TId}.GetMeta" /> for the specified resource.
        /// </summary>
        IDictionary<string, object> GetMeta(Type resourceType, IIdentifiable resourceInstance);

        /// <summary>
        /// Invokes <see cref="IResourceDefinition{TResource,TId}.OnPrepareWriteAsync" /> for the specified resource.
        /// </summary>
        Task OnPrepareWriteAsync<TResource>(TResource resource, OperationKind operationKind, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Invokes <see cref="IResourceDefinition{TResource,TId}.OnSetToOneRelationshipAsync" /> for the specified resource.
        /// </summary>
        public Task<IIdentifiable> OnSetToOneRelationshipAsync<TResource>(TResource leftResource, HasOneAttribute hasOneRelationship,
            IIdentifiable rightResourceId, OperationKind operationKind, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Invokes <see cref="IResourceDefinition{TResource,TId}.OnSetToManyRelationshipAsync" /> for the specified resource.
        /// </summary>
        public Task OnSetToManyRelationshipAsync<TResource>(TResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
            OperationKind operationKind, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Invokes <see cref="IResourceDefinition{TResource,TId}.OnAddToRelationshipAsync" /> for the specified resource.
        /// </summary>
        public Task OnAddToRelationshipAsync<TResource, TId>(TId leftResourceId, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
            CancellationToken cancellationToken)
            where TResource : class, IIdentifiable<TId>;

        /// <summary>
        /// Invokes <see cref="IResourceDefinition{TResource,TId}.OnRemoveFromRelationshipAsync" /> for the specified resource.
        /// </summary>
        public Task OnRemoveFromRelationshipAsync<TResource>(TResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
            CancellationToken cancellationToken)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Invokes <see cref="IResourceDefinition{TResource,TId}.OnWritingAsync" /> for the specified resource.
        /// </summary>
        Task OnWritingAsync<TResource>(TResource resource, OperationKind operationKind, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Invokes <see cref="IResourceDefinition{TResource,TId}.OnWriteSucceededAsync" /> for the specified resource.
        /// </summary>
        Task OnWriteSucceededAsync<TResource>(TResource resource, OperationKind operationKind, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Invokes <see cref="IResourceDefinition{TResource,TId}.OnDeserialize" /> for the specified resource.
        /// </summary>
        void OnDeserialize(IIdentifiable resource);

        /// <summary>
        /// Invokes <see cref="IResourceDefinition{TResource,TId}.OnSerialize" /> for the specified resource.
        /// </summary>
        void OnSerialize(IIdentifiable resource);
    }
}
