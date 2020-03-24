namespace JsonApiDotNetCore.Query
{
    /// <summary>
    /// Query parameter service responsible for url queries of the form ?page[size]=X&amp;page[number]=Y
    /// </summary>
    public interface IPageService : IQueryParameterService
    {
        /// <summary>
        /// Gets the requested or default page size
        /// </summary>
        int PageSize { get; }
        /// <summary>
        /// The page requested. Note that the page number is one-based.
        /// </summary>
        int CurrentPage { get; }
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
        /// <summary>
        /// What the total records are for this output
        /// </summary>
        int? TotalRecords { get; set; }
    }
}
