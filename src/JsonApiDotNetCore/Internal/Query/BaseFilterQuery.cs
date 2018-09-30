using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using System;

namespace JsonApiDotNetCore.Internal.Query
{
    public class BaseFilterQuery : BaseAttrQuery
    {
        public BaseFilterQuery(
            IJsonApiContext jsonApiContext,
            string relationship,
            string attribute,
            string value,
            FilterOperations op)
        : base(jsonApiContext, relationship, attribute)
        {
            PropertyValue = value;
            FilterOperation = op;
        }

        [Obsolete("To resolve operation use enum typed " + nameof(FilterQuery.OperationType) + " property of "+ nameof(FilterQuery) +" class")]
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
