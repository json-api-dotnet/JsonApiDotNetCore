using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Query
{
    /// <summary>
    /// Base interface that all query parameter services should inherit.
    /// </summary>
    public interface IQueryParameterService
    {
        /// <summary>
        /// Parses the value of the query parameter. Invoked in the middleware.
        /// </summary>
        /// <param name="queryParameter">the value of the query parameter as retrieved from the url</param>
        void Parse(KeyValuePair<string, StringValues> queryParameter);
        /// <summary>
        /// The name of the query parameter as matched in the URL query string.
        /// </summary>
        string Name { get; }
    }
}
