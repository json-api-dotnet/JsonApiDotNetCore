using System;
using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Internal.Query
{
    public class AttrFilterQuery : BaseFilterQuery
    {
        public AttrFilterQuery(
            IJsonApiContext jsonApiContext,
            FilterQuery filterQuery)
            : base(jsonApiContext, 
                  null,
                  filterQuery.Attribute, 
                  filterQuery.Value, 
                  filterQuery.OperationType)
        {
            if (Attribute == null)
                throw new JsonApiException(400, $"'{filterQuery.Attribute}' is not a valid attribute.");

            if (Attribute.IsFilterable == false)
                throw new JsonApiException(400, $"Filter is not allowed for attribute '{Attribute.PublicAttributeName}'.");

            FilteredAttribute = Attribute;
        }

        [Obsolete("Use " + nameof(Attribute) + " property of " + nameof(BaseAttrQuery) + "class. This property is shared for all AttrQuery and RelatedAttrQuery (filter,sort..) implementations.")]
        public AttrAttribute FilteredAttribute { get; set; }
    }
}
