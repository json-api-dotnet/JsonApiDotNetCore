using System.Collections.Generic;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCore.Resources
{
    /// <summary>
    /// Used internally to track resource extensibility endpoints. Do not implement this interface directly.
    /// </summary>
    public interface IResourceDefinition
    {
        IReadOnlyCollection<IncludeElementExpression> OnApplyIncludes(IReadOnlyCollection<IncludeElementExpression> existingIncludes);
        FilterExpression OnApplyFilter(FilterExpression existingFilter);
        SortExpression OnApplySort(SortExpression existingSort);
        PaginationExpression OnApplyPagination(PaginationExpression existingPagination);
        SparseFieldSetExpression OnApplySparseFieldSet(SparseFieldSetExpression existingSparseFieldSet);
        object GetQueryableHandlerForQueryStringParameter(string parameterName);
    }
}
