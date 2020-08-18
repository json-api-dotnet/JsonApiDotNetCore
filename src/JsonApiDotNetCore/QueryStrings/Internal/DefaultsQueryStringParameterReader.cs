using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Errors;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.QueryStrings.Internal
{
    /// <inheritdoc/>
    public class DefaultsQueryStringParameterReader : IDefaultsQueryStringParameterReader
    {
        private readonly IJsonApiOptions _options;

        /// <inheritdoc/>
        public DefaultValueHandling SerializerDefaultValueHandling { get; private set; }

        public DefaultsQueryStringParameterReader(IJsonApiOptions options)
        {
            SerializerDefaultValueHandling = options.SerializerSettings.DefaultValueHandling;
            _options = options;
        }

        /// <inheritdoc/>
        public bool IsEnabled(DisableQueryStringAttribute disableQueryStringAttribute)
        {
            return _options.AllowQueryStringOverrideForSerializerDefaultValueHandling &&
                   !disableQueryStringAttribute.ContainsParameter(StandardQueryStringParameters.Defaults);
        }

        /// <inheritdoc/>
        public bool CanRead(string parameterName)
        {
            return parameterName == "defaults";
        }

        /// <inheritdoc/>
        public void Read(string parameterName, StringValues parameterValue)
        {
            if (!bool.TryParse(parameterValue, out var result))
            {
                throw new InvalidQueryStringParameterException(parameterName,
                    "The specified defaults is invalid.",
                    $"The value '{parameterValue}' must be 'true' or 'false'.");
            }

            SerializerDefaultValueHandling = result ? DefaultValueHandling.Include : DefaultValueHandling.Ignore;
        }
    }
}
