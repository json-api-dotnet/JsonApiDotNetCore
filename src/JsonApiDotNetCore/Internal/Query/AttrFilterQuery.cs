using System;
using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Internal.Query
{
    public class AttrFilterQuery : BaseFilterQuery
    {
        private readonly IJsonApiContext _jsonApiContext;

        public AttrFilterQuery(
            IJsonApiContext jsonApiContext,
            FilterQuery filterQuery)
        {
            _jsonApiContext = jsonApiContext;

            var attribute = GetAttribute(filterQuery.Attribute);

            if (attribute == null)
                return; // we don't want to throw...we should allow custom filter implementations

            IsAttribute = true;

            if (attribute.IsFilterable == false)
                throw new JsonApiException(400, $"Filter is not allowed for attribute '{attribute.PublicAttributeName}'.");

            FilteredAttribute = attribute;
            PropertyValue = filterQuery.Value;
            FilterOperation = GetFilterOperation(filterQuery.Operation);
        }

        public AttrAttribute FilteredAttribute { get; }
        public string PropertyValue { get; }
        public FilterOperations FilterOperation { get; }

        /// <summary>
        /// Whether or not the filter is an actual attribute on the model.
        /// We use this to allow custom filters that have to be handled by the application.
        /// </summary>
        internal bool IsAttribute { get; set; }

        private AttrAttribute GetAttribute(string attribute) =>
            _jsonApiContext.RequestEntity.Attributes.FirstOrDefault(attr => attr.Is(attribute));
    }
}
