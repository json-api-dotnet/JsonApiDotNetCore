using System;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Queries
{
    /// <inheritdoc />
    internal sealed class PaginationContext : IPaginationContext
    {
        /// <inheritdoc />
        public PageNumber PageNumber { get; set; }

        /// <inheritdoc />
        public PageSize PageSize { get; set; }

        /// <inheritdoc />
        public bool IsPageFull { get; set; }

        /// <inheritdoc />
        public int? TotalResourceCount { get; set; }

        /// <inheritdoc />
        public int? TotalPageCount =>
            TotalResourceCount == null || PageSize == null ? null : (int?)Math.Ceiling((decimal)TotalResourceCount.Value / PageSize.Value);
    }
}
