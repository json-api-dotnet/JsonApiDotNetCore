namespace JsonApiDotNetCore.Internal
{
    public class PageManager
    {
        public int TotalRecords { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public bool IsPaginated { get { return PageSize > 0; } }
    }
}
