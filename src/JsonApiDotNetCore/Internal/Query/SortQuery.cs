using JsonApiDotNetCore.Models;
using System;

namespace JsonApiDotNetCore.Internal.Query
{
    public class SortQuery : BaseAttrQuery
    {
        public SortQuery(SortDirection direction, AttrAttribute sortedAttribute)
            : base(null, sortedAttribute)
        {
            Direction = direction;
            SortedAttribute = sortedAttribute;
            if (Attr.IsSortable == false)
                throw new JsonApiException(400, $"Sort is not allowed for attribute '{Attr.PublicAttributeName}'.");
        }

        public SortQuery(SortDirection direction, RelationshipAttribute relationship, AttrAttribute sortedAttribute)
            : base(relationship, sortedAttribute)
        {
            Direction = direction;
            SortedAttribute = sortedAttribute;
            if (Attr.IsSortable == false)
                throw new JsonApiException(400, $"Sort is not allowed for attribute '{Attr.PublicAttributeName}'.");
        }

        public SortDirection Direction { get; set; }
        [Obsolete("Use generic Attr property of BaseAttrQuery instead")]
        public AttrAttribute SortedAttribute { get; set; }
    }
}
