using System;

namespace JsonApiDotNetCore.Internal.Query
{
    /// <summary>
    /// Wrapper class for filter queries. Provides the internals
    /// with metadata it needs to perform the url filter queries on the targeted dataset.
    /// </summary>
    public class FilterQueryContext : BaseQueryContext<FilterQuery>
    {
        public FilterQueryContext(FilterQuery query) : base(query) { }
        public object CustomQuery { get; set; }
        public string Value => Query.Value;
        public FilterOperation Operation
        {
            get
            {
                if (!Enum.TryParse<FilterOperation>(Query.Operation, out var result))
                    return FilterOperation.eq;
                return result;
            }
        }
    }
}
