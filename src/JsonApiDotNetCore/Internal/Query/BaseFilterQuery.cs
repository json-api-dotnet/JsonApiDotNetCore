using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Services;
using System;

namespace JsonApiDotNetCore.Internal.Query
{
    /// <summary>
    /// Is the base for all filter queries
    /// </summary>
    public class BaseFilterQuery : BaseAttrQuery
    {
        public BaseFilterQuery(
            IRequestManager requestManager,
            IResourceGraph resourceGraph,
            FilterQuery filterQuery)
        : base(requestManager, resourceGraph, filterQuery)
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
