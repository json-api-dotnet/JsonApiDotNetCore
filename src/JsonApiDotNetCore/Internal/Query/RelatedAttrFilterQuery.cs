using System;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Internal.Query
{
    public class RelatedAttrFilterQuery : BaseFilterQuery
    {
        public RelatedAttrFilterQuery(
            IRequestManager requestManager,
            IResourceGraph resourceGraph,
            FilterQuery filterQuery)
            : base(requestManager: requestManager,
                  resourceGraph: resourceGraph,
                  filterQuery: filterQuery)
        {
            if (Relationship == null)
                throw new JsonApiException(400, $"{filterQuery.Relationship} is not a valid relationship on {requestManager.GetRequestResource().EntityName}.");

            if (Attribute == null)
                throw new JsonApiException(400, $"'{filterQuery.Attribute}' is not a valid attribute.");

            if (Attribute.IsFilterable == false)
                throw new JsonApiException(400, $"Filter is not allowed for attribute '{Attribute.PublicAttributeName}'.");
        }
    }
}
