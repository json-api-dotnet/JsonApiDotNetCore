using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Queries
{
    /// <summary>
    /// Tracks values used for pagination, which is a combined effort from options, query string parsing and fetching the total number of rows.
    /// </summary>
    public interface IPaginationContext
    {
        /// <summary>
        /// The value 1, unless specified from query string. Never null. Cannot be higher than options.MaximumPageNumber.
        /// </summary>
        PageNumber PageNumber { get; set; }

        /// <summary>
        /// The default page size from options, unless specified in query string. Can be <c>null</c>, which means no paging. Cannot be higher than
        /// options.MaximumPageSize.
        /// </summary>
        PageSize PageSize { get; set; }

        /// <summary>
        /// Indicates whether the number of resources on the current page equals the page size. When <c>true</c>, a subsequent page might exist (assuming
        /// <see cref="TotalResourceCount" /> is unknown).
        /// </summary>
        bool IsPageFull { get; set; }

        /// <summary>
        /// The total number of resources. <c>null</c> when <see cref="IJsonApiOptions.IncludeTotalResourceCount" /> is set to <c>false</c>.
        /// </summary>
        int? TotalResourceCount { get; set; }

        /// <summary>
        /// The total number of resource pages. <c>null</c> when <see cref="IJsonApiOptions.IncludeTotalResourceCount" /> is set to <c>false</c> or
        /// <see cref="PageSize" /> is <c>null</c>.
        /// </summary>
        int? TotalPageCount { get; }
    }
}
