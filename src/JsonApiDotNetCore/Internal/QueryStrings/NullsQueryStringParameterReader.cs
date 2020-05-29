using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Exceptions;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Internal.QueryStrings
{
    public interface INullsQueryStringParameterReader : IQueryStringParameterReader
    {
        /// <summary>
        /// Contains the effective value of default configuration and query string override, after parsing has occured.
        /// </summary>
        NullValueHandling SerializerNullValueHandling { get; }
    }

    public class NullsQueryStringParameterReader : INullsQueryStringParameterReader
    {
        private readonly IJsonApiOptions _options;

        /// <inheritdoc/>
        public NullValueHandling SerializerNullValueHandling { get; private set; }

        public NullsQueryStringParameterReader(IJsonApiOptions options)
        {
            SerializerNullValueHandling = options.SerializerSettings.NullValueHandling;
            _options = options;
        }

        public bool IsEnabled(DisableQueryAttribute disableQueryAttribute)
        {
            return _options.AllowQueryStringOverrideForSerializerNullValueHandling &&
                   !disableQueryAttribute.ContainsParameter(StandardQueryStringParameters.Nulls);
        }

        public bool CanRead(string parameterName)
        {
            return parameterName == "nulls";
        }

        public void Read(string parameterName, StringValues parameterValue)
        {
            if (!bool.TryParse(parameterValue, out var result))
            {
                throw new InvalidQueryStringParameterException(parameterName,
                    "The specified nulls is invalid.",
                    $"The value '{parameterValue}' must be 'true' or 'false'.");
            }

            SerializerNullValueHandling = result ? NullValueHandling.Include : NullValueHandling.Ignore;
        }
    }
}
