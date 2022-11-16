using System;
using System.Net;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Internal.Query
{
    public class RelatedAttrFilterQuery : BaseFilterQuery
    {
        public RelatedAttrFilterQuery(
            IJsonApiContext jsonApiContext,
            FilterQuery filterQuery)
            :base(jsonApiContext, filterQuery)
        {
            if (Relationship == null)
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                    {
                        Title = $"{filterQuery.Relationship} is not a valid relationship on {jsonApiContext.RequestEntity.EntityName}."
                    });

            if (Attribute == null)
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                    {
                        Title = $"'{filterQuery.Attribute}' is not a valid attribute."
                    });

            if (Attribute.IsFilterable == false)
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                    {
                        Title = $"Filter is not allowed for attribute '{Attribute.PublicAttributeName}'."
                    });

            FilteredRelationship = Relationship;
            FilteredAttribute = Attribute;
        }

        [Obsolete("Use " + nameof(Attribute) + " instead.")]
        public AttrAttribute FilteredAttribute { get; set; }

        [Obsolete("Use " + nameof(Relationship) + " instead.")]
        public RelationshipAttribute FilteredRelationship { get; set; }
    }
}
