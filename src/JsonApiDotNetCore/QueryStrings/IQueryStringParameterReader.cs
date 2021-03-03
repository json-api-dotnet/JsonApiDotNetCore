using JsonApiDotNetCore.Controllers.Annotations;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.QueryStrings
{
    /// <summary>
    /// The interface to implement for processing a specific type of query string parameter.
    /// </summary>
    public interface IQueryStringParameterReader
    {
        /// <summary>
        /// Indicates whether usage of this query string parameter is blocked using <see cref="DisableQueryStringAttribute" /> on a controller.
        /// </summary>
        bool IsEnabled(DisableQueryStringAttribute disableQueryStringAttribute);

        /// <summary>
        /// Indicates whether this reader can handle the specified query string parameter.
        /// </summary>
        bool CanRead(string parameterName);

        /// <summary>
        /// Reads the value of the query string parameter.
        /// </summary>
        void Read(string parameterName, StringValues parameterValue);
    }
}
