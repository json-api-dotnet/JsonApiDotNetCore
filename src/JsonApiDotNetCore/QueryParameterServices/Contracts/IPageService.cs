namespace JsonApiDotNetCore.Query
{
    /// <summary>
    /// Query parameter service responsible for url queries of the form ?page[size]=X&page[number]=Y
    /// </summary>
    public interface IPageService : IQueryParameterService
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
        /// The page requested. Note that the page number is one-based.
        /// </summary>
        int CurrentPage { get; set; }
        /// <summary>
        /// Total amount of pages for request
        /// </summary>
        int TotalPages { get; }
        /// <summary>
        /// Denotes if pagination is possible for the current request
        /// </summary>
        bool CanPaginate { get; }
        /// <summary>
        /// Denotes if pagination is backwards
        /// </summary>
        bool Backwards { get; }
    }
}
