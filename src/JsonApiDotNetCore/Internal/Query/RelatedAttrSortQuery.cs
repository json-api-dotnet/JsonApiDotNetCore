using JsonApiDotNetCore.Internal.Contracts;

namespace JsonApiDotNetCore.Internal.Query
{
    public class RelatedAttrSortQuery : BaseAttrQuery
    {
        public RelatedAttrSortQuery(ContextEntity primaryResource,
                                    IContextEntityProvider provider,
                                    SortQuery sortQuery) : base(primaryResource, provider, sortQuery)
        {
            if (Relationship == null)
                throw new JsonApiException(400, $"{sortQuery.Relationship} is not a valid relationship on {primaryResource.EntityName}.");

            if (Attribute == null)
                throw new JsonApiException(400, $"'{sortQuery.Attribute}' is not a valid attribute.");

            if (Attribute.IsSortable == false)
                throw new JsonApiException(400, $"Sort is not allowed for attribute '{Attribute.PublicAttributeName}'.");

            Direction = sortQuery.Direction;
        }

        public SortDirection Direction { get; }
    }
}
