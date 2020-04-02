using JsonApiDotNetCore.Controllers;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Query
{
    /// <summary>
    /// The interface to implement for parsing specific query string parameters.
    /// </summary>
    public interface IQueryParameterService
    {
        /// <summary>
        /// Indicates whether using this service is blocked using <see cref="DisableQueryAttribute"/> on a controller.
        /// </summary>
        bool IsEnabled(DisableQueryAttribute disableQueryAttribute);

        /// <summary>
        /// Indicates whether this service supports parsing the specified query string parameter.
        /// </summary>
        bool CanParse(string parameterName);

        /// <summary>
        /// Parses the value of the query string parameter.
        /// </summary>
        void Parse(string parameterName, StringValues parameterValue);
    }
}
