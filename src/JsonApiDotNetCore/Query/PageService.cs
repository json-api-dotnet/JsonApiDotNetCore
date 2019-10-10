using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Query;

namespace JsonApiDotNetCore.Query

{
    public class PageService : IPageQueryService
    {
        private IJsonApiOptions _options;

        public PageService(IJsonApiOptions options)
        {
            _options = options;
            DefaultPageSize = _options.DefaultPageSize;
            PageSize = _options.DefaultPageSize;
        }
        /// <inheritdoc/>
        public int? TotalRecords { get; set; }
        /// <inheritdoc/>
        public int PageSize { get; set; }
        /// <inheritdoc/>
        public int DefaultPageSize { get; set; } // I think we shouldnt expose this
        /// <inheritdoc/>
        public int CurrentPage { get; set; }
        /// <inheritdoc/>
        public int TotalPages => (TotalRecords == null) ? -1 : (int)Math.Ceiling(decimal.Divide(TotalRecords.Value, PageSize));
        /// <inheritdoc/>
        public bool ShouldPaginate()
        {
            return (PageSize > 0) || ((CurrentPage == 1 || CurrentPage == 0) && TotalPages <= 0);
        }
    }
}
