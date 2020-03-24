namespace JsonApiDotNetCore.Internal.Query
{
    /// <summary>
    /// Wrapper class for sort queries. Provides the internals
    /// with metadata it needs to perform the url sort queries on the targeted dataset.
    /// </summary>
    public class SortQueryContext : BaseQueryContext<SortQuery>
    {
        public SortQueryContext(SortQuery sortQuery) : base(sortQuery) { }
        
        public SortDirection Direction => Query.Direction;
    }
}
