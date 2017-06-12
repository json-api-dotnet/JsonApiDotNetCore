using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Internal.Query
{
    public class AttrFilterQuery : BaseFilterQuery
    {
        private readonly IJsonApiContext _jsonApiContext;

        public AttrFilterQuery(
            IJsonApiContext jsonApiCopntext,
            FilterQuery filterQuery)
        {
            _jsonApiContext = jsonApiCopntext;

            var attribute = GetAttribute(filterQuery.Key);

            FilteredAttribute = attribute ?? throw new JsonApiException(400, $"{filterQuery.Key} is not a valid property.");
            PropertyValue = filterQuery.Value;
            FilterOperation = GetFilterOperation(filterQuery.Operation);
        }

        public AttrAttribute FilteredAttribute { get; set; }
        public string PropertyValue { get; set; }
        public FilterOperations FilterOperation { get; set; }

        private AttrAttribute GetAttribute(string propertyName)
        {
            return _jsonApiContext.RequestEntity.Attributes
                .FirstOrDefault(attr =>
                    attr.InternalAttributeName.ToLower() == propertyName.ToLower()
            );
        }
    }
}