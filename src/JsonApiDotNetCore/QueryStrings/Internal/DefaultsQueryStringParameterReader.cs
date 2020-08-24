using System;
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
            _options = options ?? throw new ArgumentNullException(nameof(options));
            SerializerDefaultValueHandling = options.SerializerSettings.DefaultValueHandling;
        }

        /// <inheritdoc/>
        public virtual bool IsEnabled(DisableQueryStringAttribute disableQueryStringAttribute)
        {
            if (disableQueryStringAttribute == null) throw new ArgumentNullException(nameof(disableQueryStringAttribute));

            return _options.AllowQueryStringOverrideForSerializerDefaultValueHandling &&
                   !disableQueryStringAttribute.ContainsParameter(StandardQueryStringParameters.Defaults);
        }

        /// <inheritdoc/>
        public virtual bool CanRead(string parameterName)
        {
            return parameterName == "defaults";
        }

        /// <inheritdoc/>
        public virtual void Read(string parameterName, StringValues parameterValue)
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
