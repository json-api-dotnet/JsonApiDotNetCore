using System;
using System.Net;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Internal.Query
{
    public class AttrFilterQuery : BaseFilterQuery
    {
        public AttrFilterQuery(
            IJsonApiContext jsonApiContext,
            FilterQuery filterQuery)
            : base(jsonApiContext, filterQuery)
        {
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

            FilteredAttribute = Attribute;
        }

        [Obsolete("Use " + nameof(BaseAttrQuery.Attribute) + " insetad.")]
        public AttrAttribute FilteredAttribute { get; set; }
    }
}
