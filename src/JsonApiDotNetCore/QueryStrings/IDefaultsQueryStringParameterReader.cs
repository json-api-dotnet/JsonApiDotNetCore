using Newtonsoft.Json;

namespace JsonApiDotNetCore.QueryStrings
{
    /// <summary>
    /// Reads the 'defaults' query string parameter.
    /// </summary>
    public interface IDefaultsQueryStringParameterReader : IQueryStringParameterReader
    {
        /// <summary>
        /// Contains the effective value of default configuration and query string override, after parsing has occurred.
        /// </summary>
        DefaultValueHandling SerializerDefaultValueHandling { get; }
    }
}
