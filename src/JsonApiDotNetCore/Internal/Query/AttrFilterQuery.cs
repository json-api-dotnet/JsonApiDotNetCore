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

            if(attribute == null)
                throw new JsonApiException(400, $"'{filterQuery.Attribute}' is not a valid attribute.");

            if(attribute.IsFilterable == false)
                throw new JsonApiException(400, $"Filter is not allowed for attribute '{attribute.PublicAttributeName}'.");

            FilteredAttribute = attribute;
            PropertyValue = filterQuery.Value;
            FilterOperation = GetFilterOperation(filterQuery.Operation);
        }

        public AttrAttribute FilteredAttribute { get; }
        public string PropertyValue { get; }
        public FilterOperations FilterOperation { get; }

        private AttrAttribute GetAttribute(string attribute) =>  
            _jsonApiContext.RequestEntity.Attributes.FirstOrDefault(
                attr => string.Equals(attr.PublicAttributeName, attribute, StringComparison.OrdinalIgnoreCase)
            );
    }
}
