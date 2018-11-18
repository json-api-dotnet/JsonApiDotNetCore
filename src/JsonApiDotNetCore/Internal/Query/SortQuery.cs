using JsonApiDotNetCore.Models;
using System;

namespace JsonApiDotNetCore.Internal.Query
{
    /// <summary>
    /// An internal representation of the raw sort query.
    /// </summary>
    public class SortQuery : BaseQuery
    {
        [Obsolete("Use constructor overload (SortDirection, string) instead. The string should be the publicly exposed attribute name.", error: true)]
        public SortQuery(SortDirection direction, AttrAttribute sortedAttribute)
            : base(sortedAttribute.PublicAttributeName) { }

        public SortQuery(SortDirection direction, string attribute)
            : base(attribute)
        {
            Direction = direction;
        }

        /// <summary>
        /// Direction the sort should be applied
        /// </summary>
        public SortDirection Direction { get; set; }

        [Obsolete("Use string based Attribute instead.", error: true)]
        public AttrAttribute SortedAttribute { get; set; }
    }
}
