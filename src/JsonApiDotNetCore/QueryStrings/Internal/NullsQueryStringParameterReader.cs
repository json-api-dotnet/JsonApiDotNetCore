using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Errors;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.QueryStrings.Internal
{
    /// <inheritdoc/>
    public class NullsQueryStringParameterReader : INullsQueryStringParameterReader
    {
        private readonly IJsonApiOptions _options;

        /// <inheritdoc/>
        public NullValueHandling SerializerNullValueHandling { get; private set; }

        public NullsQueryStringParameterReader(IJsonApiOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            SerializerNullValueHandling = options.SerializerSettings.NullValueHandling;
        }

        /// <inheritdoc/>
        public bool IsEnabled(DisableQueryStringAttribute disableQueryStringAttribute)
        {
            if (disableQueryStringAttribute == null) throw new ArgumentNullException(nameof(disableQueryStringAttribute));

            return _options.AllowQueryStringOverrideForSerializerNullValueHandling &&
                   !disableQueryStringAttribute.ContainsParameter(StandardQueryStringParameters.Nulls);
        }

        /// <inheritdoc/>
        public bool CanRead(string parameterName)
        {
            return parameterName == "nulls";
        }

        /// <inheritdoc/>
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
