using System;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Internal
{
    public class PageManager
    {
        public long TotalRecords { get; set; }
        public int PageSize { get; set; }
        public int DefaultPageSize { get; set; }
        public int CurrentPage { get; set; }
        public bool IsPaginated => PageSize > 0;
        public int TotalPages => (TotalRecords == 0) ? -1 : (int)Math.Ceiling(decimal.Divide(TotalRecords, PageSize));

        public RootLinks GetPageLinks(LinkBuilder linkBuilder)
        {
            if (ShouldIncludeLinksObject())
                return null;

            var rootLinks = new RootLinks();

            if (CurrentPage > 1)
                rootLinks.First = linkBuilder.GetPageLink(1, PageSize);

            if (CurrentPage > 1)
                rootLinks.Prev = linkBuilder.GetPageLink(CurrentPage - 1, PageSize);

            if (CurrentPage < TotalPages)
                rootLinks.Next = linkBuilder.GetPageLink(CurrentPage + 1, PageSize);

            if (TotalPages > 0)
                rootLinks.Last = linkBuilder.GetPageLink(TotalPages, PageSize);

            return rootLinks;
        }

        private bool ShouldIncludeLinksObject() => (!IsPaginated || ((CurrentPage == 1 || CurrentPage == 0) && TotalPages <= 0));
    }
}
