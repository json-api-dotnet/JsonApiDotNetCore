using System;
using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Internal.Query
{
    public class RelatedAttrFilterQuery : BaseFilterQuery
    {
        public RelatedAttrFilterQuery(
            IJsonApiContext jsonApiContext,
            FilterQuery filterQuery)
            :base(jsonApiContext, filterQuery.Relationship, filterQuery.Attribute, filterQuery.Value, filterQuery.OperationType)
        {
            if (Relationship == null)
                throw new JsonApiException(400, $"{filterQuery.Relationship} is not a valid relationship on {jsonApiContext.RequestEntity.EntityName}.");

            if (Attribute == null)
                throw new JsonApiException(400, $"'{filterQuery.Attribute}' is not a valid attribute.");

            if (Attribute.IsFilterable == false)
                throw new JsonApiException(400, $"Filter is not allowed for attribute '{Attribute.PublicAttributeName}'.");

            FilteredRelationship = Relationship;
            FilteredAttribute = Attribute;
        }

        [Obsolete("Use " + nameof(Attribute) + " property. It's shared for all implementations of BaseAttrQuery(better sort, filter) handling")]
        public AttrAttribute FilteredAttribute { get; set; }

        [Obsolete("Use " + nameof(Relationship) + " property. It's shared for all implementations of BaseAttrQuery(better sort, filter) handling")]
        public RelationshipAttribute FilteredRelationship { get; set; }
    }
}
