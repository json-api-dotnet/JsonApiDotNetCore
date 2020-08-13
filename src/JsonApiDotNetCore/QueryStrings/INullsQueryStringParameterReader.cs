using Newtonsoft.Json;

namespace JsonApiDotNetCore.QueryStrings
{
    /// <summary>
    /// Reads the 'nulls' query string parameter.
    /// </summary>
    public interface INullsQueryStringParameterReader : IQueryStringParameterReader
    {
        /// <summary>
        /// Contains the effective value of default configuration and query string override, after parsing has occured.
        /// </summary>
        NullValueHandling SerializerNullValueHandling { get; }
    }
}
