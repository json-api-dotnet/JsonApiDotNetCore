using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Internal.Query
{
    public class RelatedAttrFilterQuery : BaseFilterQuery
    {
        private readonly IJsonApiContext _jsonApiContext;
        
        public RelatedAttrFilterQuery(
            IJsonApiContext jsonApiContext,
            FilterQuery filterQuery)
        {
            _jsonApiContext = jsonApiContext;

            var relationshipArray = filterQuery.Attribute.Split('.');
            var relationship = GetRelationship(relationshipArray[0]);
            if (relationship == null)
                throw new JsonApiException(400, $"{relationshipArray[1]} is not a valid relationship on {relationshipArray[0]}.");

            var attribute = GetAttribute(relationship, relationshipArray[1]);
            if (attribute == null)
                throw new JsonApiException(400, $"'{filterQuery.Attribute}' is not a valid attribute.");

            if (attribute.IsFilterable == false)
                throw new JsonApiException(400, $"Filter is not allowed for attribute '{attribute.PublicAttributeName}'.");

            FilteredRelationship = relationship;
            FilteredAttribute = attribute;
            PropertyValue = filterQuery.Value;
            FilterOperation = GetFilterOperation(filterQuery.Operation);
        }

        public AttrAttribute FilteredAttribute { get; set; }
        public string PropertyValue { get; set; }
        public FilterOperations FilterOperation { get; set; }
        public RelationshipAttribute FilteredRelationship { get; }

        private RelationshipAttribute GetRelationship(string propertyName)
            => _jsonApiContext.RequestEntity.Relationships.FirstOrDefault(r => r.Is(propertyName));

        private AttrAttribute GetAttribute(RelationshipAttribute relationship, string attribute)
        {
            var relatedContextExntity = _jsonApiContext.ResourceGraph.GetContextEntity(relationship.Type);
            return relatedContextExntity.Attributes
              .FirstOrDefault(a => a.Is(attribute));
        }
    }
}
