using JsonApiDotNetCore.Services;
using System;

namespace JsonApiDotNetCore.Internal.Query
{
    public class BaseFilterQuery : BaseAttrQuery
    {
        public BaseFilterQuery(
            IJsonApiContext jsonApiContext,
            FilterQuery filterQuery)
        : base(jsonApiContext.RequestManager, jsonApiContext.ResourceGraph, filterQuery)
        {
            PropertyValue = filterQuery.Value;
            FilterOperation = GetFilterOperation(filterQuery.Operation);
        }

        public string PropertyValue { get; }
        public FilterOperations FilterOperation { get; }

        private FilterOperations GetFilterOperation(string prefix)
        {
            if (prefix.Length == 0) return FilterOperations.eq;

            if (Enum.TryParse(prefix, out FilterOperations opertion) == false)
                throw new JsonApiException(400, $"Invalid filter prefix '{prefix}'");

            return opertion;
        }

    }
}
