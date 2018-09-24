using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Internal.Query
{
    public class AttrFilterQuery : AttrQuery
    {
        public AttrFilterQuery(
            IJsonApiContext jsonApiContext,
            FilterQuery filterQuery)
            :base(jsonApiContext, filterQuery)
        {
            if (Attribute.IsFilterable == false)
                throw new JsonApiException(400, $"Filter is not allowed for attribute '{Attribute.PublicAttributeName}'.");

            PropertyValue = filterQuery.Value;
            FilterOperation = FilterOperations.GetFilterOperation(filterQuery.Operation);
        }

        public string PropertyValue { get; }
        public FilterOperationsEnum FilterOperation { get; }

    }
}
