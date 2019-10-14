namespace JsonApiDotNetCore.Internal.Query
{
    public class SortQueryContext : BaseQueryContext<SortQuery>
    {
        public SortQueryContext(SortQuery sortQuery) : base(sortQuery) { }
        public SortDirection Direction => Query.Direction;
    }
}
