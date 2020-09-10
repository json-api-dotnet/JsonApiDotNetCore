using System;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents a pagination, produced from <see cref="PaginationQueryStringValueExpression"/>.
    /// </summary>
    public class PaginationExpression : QueryExpression
    {
        public PageNumber PageNumber { get; }
        public PageSize PageSize { get; }

        public PaginationExpression(PageNumber pageNumber, PageSize pageSize)
        {
            PageNumber = pageNumber ?? throw new ArgumentNullException(nameof(pageNumber));
            PageSize = pageSize;
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitPagination(this, argument);
        }

        public override string ToString()
        {
            return PageSize != null ? $"Page number: {PageNumber}, size: {PageSize}" : "(none)";
        }
    }
}
