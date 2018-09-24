using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Internal.Query
{
    public class AttrSortQuery : AttrQuery
    {
        public AttrSortQuery(
            IJsonApiContext jsonApiContext,
            SortQuery sortQuery)
            :base(jsonApiContext, sortQuery)
        {
            if (Attribute.IsSortable == false)
                throw new JsonApiException(400, $"Sort is not allowed for attribute '{Attribute.PublicAttributeName}'.");

            Direction = sortQuery.Direction;
        }

        public SortDirection Direction { get; set; }

    }
}
