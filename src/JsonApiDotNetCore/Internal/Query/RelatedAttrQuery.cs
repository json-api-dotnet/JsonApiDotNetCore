using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using System;
using System.Linq;

namespace JsonApiDotNetCore.Internal.Query
{
    public class RelatedAttrQuery
    {
        private readonly IJsonApiContext _jsonApiContext;

        public RelatedAttrQuery(IJsonApiContext jsonApiContext, QueryAttribute query)
        {
            _jsonApiContext = jsonApiContext;

            RelationshipAttribute = GetRelationshipAttribute(query.RelationshipAttribute);
            Attribute = GetAttribute(RelationshipAttribute, query.Attribute);
        }

        public AttrAttribute Attribute { get; }
        public RelationshipAttribute RelationshipAttribute { get; }

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
