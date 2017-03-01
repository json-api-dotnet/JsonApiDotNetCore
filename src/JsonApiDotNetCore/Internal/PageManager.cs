using System;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Internal
{
    public class PageManager
    {
        public int TotalRecords { get; set; }
        public int PageSize { get; set; }
        public int DefaultPageSize { get; set; }
        public int CurrentPage { get; set; }
        public bool IsPaginated { get { return PageSize > 0; } }
        public int TotalPages { 
            get { return (TotalRecords == 0) ? -1: (int)Math.Ceiling(decimal.Divide(TotalRecords, PageSize)); }
        }

        public RootLinks GetPageLinks(LinkBuilder linkBuilder)
        {            
            if(!IsPaginated || (CurrentPage == 1 && TotalPages <= 0))
                return null;
            
            var rootLinks = new RootLinks();

            var includePageSize = DefaultPageSize != PageSize;

             if(CurrentPage > 1)
                rootLinks.First = linkBuilder.GetPageLink(1, PageSize);

            if(CurrentPage > 1)
                rootLinks.Prev = linkBuilder.GetPageLink(CurrentPage - 1, PageSize);
            
            if(CurrentPage < TotalPages)
                rootLinks.Next = linkBuilder.GetPageLink(CurrentPage + 1, PageSize);
            
            if(TotalPages > 0)
                rootLinks.Last = linkBuilder.GetPageLink(TotalPages, PageSize);

            return rootLinks;
        }
    }
}
