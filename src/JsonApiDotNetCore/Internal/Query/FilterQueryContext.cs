using System;

namespace JsonApiDotNetCore.Internal.Query
{
    public class FilterQueryContext : BaseQueryContext<FilterQuery>
    {
        public FilterQueryContext(FilterQuery query) : base(query) { }

        public string Value => Query.Value;
        public FilterOperations Operation => (FilterOperations)Enum.Parse(typeof(FilterOperations), Query.Operation);
    }

}
