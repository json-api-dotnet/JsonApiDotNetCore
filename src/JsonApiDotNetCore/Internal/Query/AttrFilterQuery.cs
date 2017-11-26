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

            FilteredAttribute = attribute ?? throw new JsonApiException(400, $"'{filterQuery.Attribute}' is not a valid attribute.");
            PropertyValue = filterQuery.Value;
            FilterOperation = GetFilterOperation(filterQuery.Operation);
        }

        public AttrAttribute FilteredAttribute { get; set; }
        public string PropertyValue { get; set; }
        public FilterOperations FilterOperation { get; set; }

        private AttrAttribute GetAttribute(string attribute) =>  
            _jsonApiContext.RequestEntity.Attributes.FirstOrDefault(
                attr => string.Equals(attr.PublicAttributeName, attribute, StringComparison.OrdinalIgnoreCase)
            );
    }
}
