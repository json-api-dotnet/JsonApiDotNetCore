using JsonApiDotNetCore.Models;
using System;

namespace JsonApiDotNetCore.Internal.Query
{
    /// <summary>
    /// An internal representation of the raw sort query.
    /// </summary>
    public class SortQuery : BaseQuery
    {
        public SortQuery(SortDirection direction, string attribute)
            : base(attribute)
        {
            Direction = direction;
        }

        /// <summary>
        /// Direction the sort should be applied
        /// </summary>
        public SortDirection Direction { get; set; }
    }
}
