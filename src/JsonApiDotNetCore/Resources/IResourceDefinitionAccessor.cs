using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Queries.Expressions;

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
    }
}
