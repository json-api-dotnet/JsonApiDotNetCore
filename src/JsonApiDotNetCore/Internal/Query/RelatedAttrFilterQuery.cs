using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Internal.Query
{
    public class RelatedAttrFilterQuery : BaseFilterQuery
    {
        private readonly IJsonApiContext _jsonApiContext;

        public RelatedAttrFilterQuery(
            IJsonApiContext jsonApiCopntext,
            FilterQuery filterQuery)
        {
            _jsonApiContext = jsonApiCopntext;

            var relationshipArray = filterQuery.Key.Split('.');

            var relationship = GetRelationship(relationshipArray[0]);
            if (relationship == null)
                throw new JsonApiException(400, $"{relationshipArray[0]} is not a valid relationship.");

            var attribute = GetAttribute(relationship, relationshipArray[1]);

            FilteredRelationship = relationship;
            FilteredAttribute = attribute ?? throw new JsonApiException(400, $"{relationshipArray[1]} is not a valid attribute on {relationshipArray[0]}.");
            PropertyValue = filterQuery.Value;
            FilterOperation = GetFilterOperation(filterQuery.Operation);
        }

        public AttrAttribute FilteredAttribute { get; set; }
        public string PropertyValue { get; set; }
        public FilterOperations FilterOperation { get; set; }
        public RelationshipAttribute FilteredRelationship { get; }

        private RelationshipAttribute GetRelationship(string propertyName)
        {
            return _jsonApiContext.RequestEntity.Relationships
              .FirstOrDefault(r => r.InternalRelationshipName.ToLower() == propertyName.ToLower());
        }

        private AttrAttribute GetAttribute(RelationshipAttribute relationship, string attribute)
        {
            var relatedContextExntity = _jsonApiContext.ContextGraph.GetContextEntity(relationship.Type);
            return relatedContextExntity.Attributes
              .FirstOrDefault(a => a.InternalAttributeName.ToLower() == attribute.ToLower());
        }
    }
}