using System.Collections.Generic;

namespace JsonApiDotNetCore.Internal.Query
{
    public class QuerySet
    {
        public List<FilterQuery> Filters { get; set; } = new List<FilterQuery>();
        public PageQuery PageQuery { get; set; } = new PageQuery();
        public List<SortQuery> SortParameters { get; set; } = new List<SortQuery>();
        public List<string> IncludedRelationships { get; set; } = new List<string>();
        public List<string> Fields { get; set; } = new List<string>();
    }
}