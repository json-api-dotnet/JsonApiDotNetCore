using JsonApiDotNetCore.Services;
using System;
using System.Net;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Internal.Query
{
    public class BaseFilterQuery : BaseAttrQuery
    {
        public BaseFilterQuery(
            IJsonApiContext jsonApiContext,
            FilterQuery filterQuery)
        : base(jsonApiContext, filterQuery)
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
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                    {
                        Title = $"Invalid filter prefix '{prefix}'"
                    });

            return opertion;
        }

    }
}
