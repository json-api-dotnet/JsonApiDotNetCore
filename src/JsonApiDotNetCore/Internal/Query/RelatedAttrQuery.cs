using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using System;
using System.Linq;

namespace JsonApiDotNetCore.Internal.Query
{
    public class RelatedAttrQuery: BaseAttrQuery
    {
        private readonly IJsonApiContext _jsonApiContext;
        private readonly bool _isAttributeOfRelationship = true;

        /// <summary>
        /// Build RelatedAttrQuery based on FilterQuery values.
        /// </summary>
        /// <param name="jsonApiContext"></param>
        /// <param name="filterQuery"></param>
        public RelatedAttrQuery(IJsonApiContext jsonApiContext, FilterQuery filterQuery)
        {
            _jsonApiContext = jsonApiContext;

            RelationshipAttribute = GetRelationshipAttribute(filterQuery.RelationshipAttribute);
            Attribute = GetAttribute(RelationshipAttribute, filterQuery.Attribute);

            if (Attribute.IsFilterable == false)
                throw new JsonApiException(400, $"Filter is not allowed for attribute '{Attribute.PublicAttributeName}'.");

            IsAttributeOfRelationship = _isAttributeOfRelationship;
            PropertyValue = filterQuery.Value;
            FilterOperation = filterQuery.OperationType;
        }

        /// <summary>
        /// Build RelatedAttrQuery based on SortQuery values.
        /// </summary>
        /// <param name="jsonApiContext"></param>
        /// <param name="sortQuery"></param>
        public RelatedAttrQuery(IJsonApiContext jsonApiContext, SortQuery sortQuery)
        {
            _jsonApiContext = jsonApiContext;

            RelationshipAttribute = GetRelationshipAttribute(sortQuery.RelationshipAttribute);
            Attribute = GetAttribute(RelationshipAttribute, sortQuery.Attribute);

            if (Attribute.IsSortable == false)
                throw new JsonApiException(400, $"Sort is not allowed for attribute '{Attribute.PublicAttributeName}'.");

            IsAttributeOfRelationship = _isAttributeOfRelationship;
            Direction = sortQuery.Direction;
        }

        private RelationshipAttribute GetRelationshipAttribute(string relationship)
        {
            try
            {
                return _jsonApiContext
                    .RequestEntity
                    .Relationships
                    .Single(attr => attr.Is(relationship));
            }
            catch (InvalidOperationException e)
            {
                throw new JsonApiException(400, $"Relationship '{relationship}' does not exist on resource '{_jsonApiContext.RequestEntity.EntityName}'", e);
            }
        }

        private AttrAttribute GetAttribute(RelationshipAttribute relationship, string attribute)
        {
            var relatedContextExntity = _jsonApiContext.ContextGraph.GetContextEntity(relationship.Type);
            try
            {               
                return relatedContextExntity.Attributes.Single(attr => attr.Is(attribute));
            }
            catch (InvalidOperationException e)
            {
                throw new JsonApiException(400, $"Attribute '{attribute}' does not exist on resource '{relatedContextExntity.EntityName}'", e);
            }
        }
    }
}
