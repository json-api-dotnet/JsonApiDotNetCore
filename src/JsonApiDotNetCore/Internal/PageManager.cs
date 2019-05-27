using System;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Internal
{
    public class PageManager : IPageManager
    {
        private ILinkBuilder _linkBuilder;
        private IJsonApiOptions _options;

        public PageManager(ILinkBuilder linkBuilder, IJsonApiOptions options, IRequestManager requestManager)
        {
            _linkBuilder = linkBuilder;
            _options = options;
            if (requestManager.QuerySet != null)
            {
                PageSize = requestManager.QuerySet?.PageQuery.PageSize !=  null ? requestManager.QuerySet.PageQuery.PageSize : _options.DefaultPageSize;
            }
            else
            {
                PageSize = _options.DefaultPageSize;
            }

            DefaultPageSize = _options.DefaultPageSize;
        }
        public int? TotalRecords { get; set; }
        public int PageSize { get; set; }
        public int DefaultPageSize { get; set; }
        public int CurrentPage { get; set; }
        public bool IsPaginated => PageSize > 0;
        public int TotalPages => (TotalRecords == null) ? -1 : (int)Math.Ceiling(decimal.Divide(TotalRecords.Value, PageSize));

        public RootLinks GetPageLinks()
        {
            if (ShouldIncludeLinksObject())
                return null;

            var rootLinks = new RootLinks();

            if (CurrentPage > 1)
                rootLinks.First = _linkBuilder.GetPageLink(1, PageSize);

            if (CurrentPage > 1)
                rootLinks.Prev = _linkBuilder.GetPageLink(CurrentPage - 1, PageSize);

            if (CurrentPage < TotalPages)
                rootLinks.Next = _linkBuilder.GetPageLink(CurrentPage + 1, PageSize);

            if (TotalPages > 0)
                rootLinks.Last = _linkBuilder.GetPageLink(TotalPages, PageSize);

            return rootLinks;
        }

        private bool ShouldIncludeLinksObject() => (!IsPaginated || ((CurrentPage == 1 || CurrentPage == 0) && TotalPages <= 0));
    }
}
