using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Internal.Query
{
    public class RelatedAttrSortQuery : BaseAttrQuery
    {
        public RelatedAttrSortQuery(
            IJsonApiContext jsonApiContext,
            SortQuery sortQuery)
            :base(jsonApiContext, sortQuery)
        {
            if (Relationship == null)
                throw new JsonApiException(400, $"{sortQuery.Relationship} is not a valid relationship on {jsonApiContext.RequestEntity.EntityName}.");

            if (Attribute == null)
                throw new JsonApiException(400, $"'{sortQuery.Attribute}' is not a valid attribute.");

            if (Attribute.IsSortable == false)
                throw new JsonApiException(400, $"Sort is not allowed for attribute '{Attribute.PublicAttributeName}'.");

            Direction = sortQuery.Direction;
        }

        public SortDirection Direction { get; }
    }
}
