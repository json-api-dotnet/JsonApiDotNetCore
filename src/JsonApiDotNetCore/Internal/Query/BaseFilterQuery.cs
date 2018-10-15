using JsonApiDotNetCore.Services;
using System;

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
            FilterOperation = filterQuery.OperationType;
        }

        [Obsolete("Use " + nameof(FilterQuery.OperationType) + " instead.")]
        protected FilterOperations GetFilterOperation(string prefix)
        {
            if (prefix.Length == 0) return FilterOperations.eq;

            if (Enum.TryParse(prefix, out FilterOperations opertion) == false)
                throw new JsonApiException(400, $"Invalid filter prefix '{prefix}'");

            return opertion;
        }

        public string PropertyValue { get; }
        public FilterOperations FilterOperation { get; }
    }
}
