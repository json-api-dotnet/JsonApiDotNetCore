using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents a pagination, produced from <see cref="PaginationQueryStringValueExpression" />.
    /// </summary>
    [PublicAPI]
    public class PaginationExpression : QueryExpression
    {
        public PageNumber PageNumber { get; }
        public PageSize PageSize { get; }

        public PaginationExpression(PageNumber pageNumber, PageSize pageSize)
        {
            ArgumentGuard.NotNull(pageNumber, nameof(pageNumber));

            PageNumber = pageNumber;
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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (PaginationExpression)obj;

            return PageNumber.Equals(other.PageNumber) && Equals(PageSize, other.PageSize);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PageNumber, PageSize);
        }
    }
}
