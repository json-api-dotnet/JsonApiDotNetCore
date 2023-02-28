using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Queries;

/// <summary>
/// Tracks values used for top-level pagination, which is a combined effort from options, query string parsing, resource definition callbacks and
/// fetching the total number of rows.
/// </summary>
public interface IPaginationContext
{
    /// <summary>
    /// The value 1, unless overridden from query string or resource definition. Should not be higher than <see cref="IJsonApiOptions.MaximumPageNumber" />.
    /// </summary>
    PageNumber PageNumber { get; set; }

    /// <summary>
    /// The default page size from options, unless overridden from query string or resource definition. Should not be higher than
    /// <see cref="IJsonApiOptions.MaximumPageSize" />. Can be <c>null</c>, which means pagination is disabled.
    /// </summary>
    PageSize? PageSize { get; set; }

    /// <summary>
    /// Indicates whether the number of resources on the current page equals the page size. When <c>true</c>, a subsequent page might exist (assuming
    /// <see cref="TotalResourceCount" /> is unknown).
    /// </summary>
    bool IsPageFull { get; set; }

    /// <summary>
    /// The total number of resources, or <c>null</c> when <see cref="IJsonApiOptions.IncludeTotalResourceCount" /> is set to <c>false</c>.
    /// </summary>
    int? TotalResourceCount { get; set; }

    /// <summary>
    /// The total number of resource pages, or <c>null</c> when <see cref="IJsonApiOptions.IncludeTotalResourceCount" /> is set to <c>false</c> or
    /// <see cref="PageSize" /> is <c>null</c>.
    /// </summary>
    int? TotalPageCount { get; }
}
