using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Internal.Query
{
    public class SortQuery: QueryAttribute
    {
        public SortQuery(SortDirection direction, string sortedAttribute)
            :base(sortedAttribute)
        {
            Direction = direction;
        }
        public SortDirection Direction { get; set; }
    }
}
