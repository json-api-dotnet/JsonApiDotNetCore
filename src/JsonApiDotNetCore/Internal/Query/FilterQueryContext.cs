using System;

namespace JsonApiDotNetCore.Internal.Query
{
    public class FilterQueryContext : BaseQueryContext<FilterQuery>
    {
        public FilterQueryContext(FilterQuery query) : base(query) { }

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
