using System;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Internal.Query
{
    /// <summary>
    /// Allows you to filter the query, via the methods shown at
    /// <see href="https://json-api-dotnet.github.io/#/filtering">HERE</see>
    /// </summary>
    public class FilterQuery : BaseQuery
    {
        /// <summary>
        /// Allows you to filter the query, via the methods shown at
        /// <see href="https://json-api-dotnet.github.io/#/filtering">HERE</see>
        /// </summary>
        /// <param name="attribute">the json attribute you want to filter on</param>
        /// <param name="value">the value this attribute should be</param>
        /// <param name="operation">possible values: eq, ne, lt, gt, le, ge, like, in (default)</param>
        public FilterQuery(string attribute, string value, string operation)
            : base(attribute)
        {
            Value = value;
            Operation = operation;
        }

        public string Value { get; set; }
        public string Operation { get; set; }

    }
}
