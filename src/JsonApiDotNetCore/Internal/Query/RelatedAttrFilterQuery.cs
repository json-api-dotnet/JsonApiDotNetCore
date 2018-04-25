using System;
using System.Linq;
using JsonApiDotNetCore.Extensions;
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
            var filterQuerySubSpans = filterQueryAttribute.SpanSplit('.');
            var subSpan1 = filterQuerySubSpans[0].ToString();
            var subSpan2 = filterQuerySubSpans[1].ToString();
            var relationship = GetRelationship(subSpan1);
            if (relationship == null)
                throw new JsonApiException(400, $"{subSpan2} is not a valid relationship on {subSpan1}.");

            var attribute = GetAttribute(relationship, subSpan2);

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
