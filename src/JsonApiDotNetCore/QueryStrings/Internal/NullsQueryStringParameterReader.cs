using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.QueryStrings.Internal
{
    /// <inheritdoc />
    [PublicAPI]
    public class NullsQueryStringParameterReader : INullsQueryStringParameterReader
    {
        private readonly IJsonApiOptions _options;

        /// <inheritdoc />
        public NullValueHandling SerializerNullValueHandling { get; private set; }

        public NullsQueryStringParameterReader(IJsonApiOptions options)
        {
            ArgumentGuard.NotNull(options, nameof(options));

            _options = options;
            SerializerNullValueHandling = options.SerializerSettings.NullValueHandling;
        }

        /// <inheritdoc />
        public virtual bool IsEnabled(DisableQueryStringAttribute disableQueryStringAttribute)
        {
            ArgumentGuard.NotNull(disableQueryStringAttribute, nameof(disableQueryStringAttribute));

            return _options.AllowQueryStringOverrideForSerializerNullValueHandling &&
                !disableQueryStringAttribute.ContainsParameter(StandardQueryStringParameters.Nulls);
        }

        /// <inheritdoc />
        public virtual bool CanRead(string parameterName)
        {
            return parameterName == "nulls";
        }

        /// <inheritdoc />
        public virtual void Read(string parameterName, StringValues parameterValue)
        {
            if (!bool.TryParse(parameterValue, out bool result))
            {
                throw new InvalidQueryStringParameterException(parameterName, "The specified nulls is invalid.",
                    $"The value '{parameterValue}' must be 'true' or 'false'.");
            }

            SerializerNullValueHandling = result ? NullValueHandling.Include : NullValueHandling.Ignore;
        }
    }
}
