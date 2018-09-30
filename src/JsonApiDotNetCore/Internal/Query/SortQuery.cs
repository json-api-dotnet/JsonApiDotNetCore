using JsonApiDotNetCore.Models;
using System;

namespace JsonApiDotNetCore.Internal.Query
{
    public class SortQuery : BaseQuery
    {
        [Obsolete("Use constructor with string attribute parameter. New constructor provides nested sort feature.")]
        public SortQuery(SortDirection direction, AttrAttribute sortedAttribute)
            :base(sortedAttribute.InternalAttributeName)
        {
            Direction = direction;
            SortedAttribute = sortedAttribute;
            if (SortedAttribute.IsSortable == false)
                throw new JsonApiException(400, $"Sort is not allowed for attribute '{SortedAttribute.PublicAttributeName}'.");
        }

        public SortQuery(SortDirection direction, string attribute)
            : base(attribute)
        {
            Direction = direction;
        }

        public SortDirection Direction { get; set; }
        [Obsolete("Use string based Attribute instead. This provides nested sort feature (e.g. ?sort=owner.first-name)")]
        public AttrAttribute SortedAttribute { get; set; }
    }
}
