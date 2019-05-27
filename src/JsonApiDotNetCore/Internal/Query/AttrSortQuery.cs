using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Internal.Query
{
    public class AttrSortQuery : BaseAttrQuery
    {
        public AttrSortQuery(IJsonApiContext jsonApiContext,SortQuery sortQuery)
            :base(jsonApiContext.RequestManager,jsonApiContext.ResourceGraph, sortQuery)
        {
            if (Attribute == null)
                throw new JsonApiException(400, $"'{sortQuery.Attribute}' is not a valid attribute.");

            if (Attribute.IsSortable == false)
                throw new JsonApiException(400, $"Sort is not allowed for attribute '{Attribute.PublicAttributeName}'.");

            Direction = sortQuery.Direction;
        }

        public SortDirection Direction { get; }
    }
}
