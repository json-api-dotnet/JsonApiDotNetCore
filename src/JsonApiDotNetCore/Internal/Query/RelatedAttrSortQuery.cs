using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Internal.Query
{
    public class RelatedAttrSortQuery : RelatedAttrQuery
    {
       
        public RelatedAttrSortQuery(
            IJsonApiContext jsonApiContext,
            SortQuery sortQuery)
            :base(jsonApiContext, sortQuery)
        {
            if (Attribute.IsSortable == false)
                throw new JsonApiException(400, $"Sort is not allowed for attribute '{Attribute.PublicAttributeName}' on relationship '{RelationshipAttribute.PublicRelationshipName}'.");

            Direction = sortQuery.Direction;
        }

        public SortDirection Direction { get; set; }

    }
}
