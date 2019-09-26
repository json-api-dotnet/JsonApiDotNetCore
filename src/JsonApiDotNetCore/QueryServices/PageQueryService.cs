using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Managers.Contracts;

namespace JsonApiDotNetCore.Internal
{
    public class PageQueryService : IPageQueryService
    {
        private IJsonApiOptions _options;

        public PageQueryService(IJsonApiOptions options)
        {
            _options = options;
            DefaultPageSize = _options.DefaultPageSize;
            PageSize = _options.DefaultPageSize;
        }
        public int? TotalRecords { get; set; }
        public int PageSize { get; set; }
        public int DefaultPageSize { get; set; } // I think we shouldnt expose this
        public int CurrentPage { get; set; }
        public bool IsPaginated => PageSize > 0;
        public int TotalPages => (TotalRecords == null) ? -1 : (int)Math.Ceiling(decimal.Divide(TotalRecords.Value, PageSize));

        public bool ShouldPaginate()
        {
            return !IsPaginated || ((CurrentPage == 1 || CurrentPage == 0) && TotalPages <= 0);
        }
    }
}
