using System;
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
            var filterQueryAttribute = filterQuery.Attribute;
            var relationshipSubSpans = new SpanSplitter(ref filterQueryAttribute, '.');
            var relationship1 = relationshipSubSpans[0].ToString();
            var relationship2 = relationshipSubSpans[1].ToString();
            var relationship = GetRelationship(relationshipSubSpans[0].ToString());
            if (relationship == null)
                throw new JsonApiException(400, $"{relationship2} is not a valid relationship on {relationship1}.");

            var attribute = GetAttribute(relationship, relationship2);

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
            var relatedContextExntity = _jsonApiContext.ContextGraph.GetContextEntity(relationship.Type);
            return relatedContextExntity.Attributes
              .FirstOrDefault(a => a.Is(attribute));
        }
    }
}
