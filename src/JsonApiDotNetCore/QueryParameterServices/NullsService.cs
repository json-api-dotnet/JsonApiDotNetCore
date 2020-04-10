using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Exceptions;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Query
{
    /// <inheritdoc/>
    public class NullsService : QueryParameterService, INullsService
    {
        private readonly IJsonApiOptions _options;

        public NullsService(IJsonApiOptions options)
        {
            OmitAttributeIfValueIsNull = options.SerializerSettings.NullValueHandling == NullValueHandling.Ignore;
            _options = options;
        }

        /// <inheritdoc/>
        public bool OmitAttributeIfValueIsNull { get; private set; }

        /// <inheritdoc/>
        public bool IsEnabled(DisableQueryAttribute disableQueryAttribute)
        {
            return _options.AllowQueryStringOverrideForSerializerNullValueHandling && 
                   !disableQueryAttribute.ContainsParameter(StandardQueryStringParameters.Nulls);
        }

        /// <inheritdoc/>
        public bool CanParse(string parameterName)
        {
            return parameterName == "nulls";
        }

        /// <inheritdoc/>
        public virtual void Parse(string parameterName, StringValues parameterValue)
        {
            if (!bool.TryParse(parameterValue, out var result))
            {
                throw new InvalidQueryStringParameterException(parameterName,
                    "The specified query string value must be 'true' or 'false'.",
                    $"The value '{parameterValue}' for parameter '{parameterName}' is not a valid boolean value.");
            }

            OmitAttributeIfValueIsNull = !result;
        }
    }
}
