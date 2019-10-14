using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Internal.Query
{


    public class SortQueryContext : BaseQueryContext<SortQuery>
    {
        public SortQueryContext(SortQuery sortQuery) : base(sortQuery)
        {
            //if (Attribute == null)
            //    throw new JsonApiException(400, $"'{sortQuery.Attribute}' is not a valid attribute.");

            //if (Attribute.IsSortable == false)
            //    throw new JsonApiException(400, $"Sort is not allowed for attribute '{Attribute.PublicAttributeName}'.");
        }

        public SortDirection Direction => Query.Direction;
    }

}
