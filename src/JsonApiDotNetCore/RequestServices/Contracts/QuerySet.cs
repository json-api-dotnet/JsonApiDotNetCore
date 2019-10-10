using System.Collections.Generic;
using JsonApiDotNetCore.Internal.Query;

namespace JsonApiDotNetCore.Managers.Contracts
{
    public class QuerySet
    {
        public List<FilterQuery> Filters { get; internal set; }
        public List<string> Fields { get; internal set; }
        public List<SortQuery> SortParameters { get; internal set; }
    }
}