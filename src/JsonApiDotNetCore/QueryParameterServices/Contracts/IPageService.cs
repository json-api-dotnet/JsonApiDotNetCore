namespace JsonApiDotNetCore.QueryServices.Contracts
{
    /// <summary>
    /// The former page manager. Needs some work.
    /// </summary>
    public interface IPageQueryService
    {
        /// <summary>
        /// What the total records are for this output
        /// </summary>
        int? TotalRecords { get; set; }
        /// <summary>
        /// How many records per page should be shown
        /// </summary>
        int PageSize { get; set; }
        /// <summary>
        /// What is the default page size
        /// </summary>
        int DefaultPageSize { get; set; }
        /// <summary>
        /// What page are we currently on
        /// </summary>
        int CurrentPage { get; set; }

        /// <summary>
        /// Total amount of pages for request
        /// </summary>
        int TotalPages { get; }

        /// <summary>
        /// Pagination is enabled
        /// </summary>
        bool ShouldPaginate();
    }
}
