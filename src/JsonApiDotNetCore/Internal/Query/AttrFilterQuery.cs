using System;
using JsonApiDotNetCore.Models;
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
                throw new JsonApiException(400, $"'{filterQuery.Attribute}' is not a valid attribute.");

            if (Attribute.IsFilterable == false)
                throw new JsonApiException(400, $"Filter is not allowed for attribute '{Attribute.PublicAttributeName}'.");

            FilteredAttribute = Attribute;
        }

        [Obsolete("Use " + nameof(BaseAttrQuery.Attribute) + " instead.")]
        public AttrAttribute FilteredAttribute { get; set; }
    }
}
