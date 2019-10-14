namespace JsonApiDotNetCore.Internal.Query
{
    public class PageQuery
    {
       public int? PageSize { get; set; }
       public int? PageOffset { get; set; } = 1;
    }
}