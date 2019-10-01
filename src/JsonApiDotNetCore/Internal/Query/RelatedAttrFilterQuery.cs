using JsonApiDotNetCore.Internal.Contracts;

namespace JsonApiDotNetCore.Internal.Query
{
    public class RelatedAttrFilterQuery : BaseFilterQuery
    {
        public RelatedAttrFilterQuery(
            ContextEntity requestResource,
            IContextEntityProvider provider,
            FilterQuery filterQuery)
            : base(requestResource, provider, filterQuery)
        {
            if (Relationship == null)
                throw new JsonApiException(400, $"{filterQuery.Relationship} is not a valid relationship on {requestResource.EntityName}.");

            if (Attribute == null)
                throw new JsonApiException(400, $"'{filterQuery.Attribute}' is not a valid attribute.");

            if (Attribute.IsFilterable == false)
                throw new JsonApiException(400, $"Filter is not allowed for attribute '{Attribute.PublicAttributeName}'.");
        }
    }
}
