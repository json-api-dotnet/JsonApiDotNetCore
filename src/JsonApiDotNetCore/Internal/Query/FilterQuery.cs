using System;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Internal.Query
{
    /// <summary>
    /// Internal representation of the raw articles?filter[X]=Y query from the URL.
    /// </summary>
    public class FilterQuery : BaseQuery
    {
        public FilterQuery(string target, string value, string operation)
            : base(target)
        {
            Value = value;
            Operation = operation;
        }

        public string Value { get; set; }
        /// <summary>
        /// See <see cref="FilterOperation"/>. Can also be a custom operation.
        /// </summary>
        public string Operation { get; set; }
    }
}
