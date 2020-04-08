using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Exceptions;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Query
{
    /// <inheritdoc/>
    public class OmitDefaultService : QueryParameterService, IOmitDefaultService
    {
        private readonly IJsonApiOptions _options;

        public OmitDefaultService(IJsonApiOptions options)
        {
            OmitAttributeIfValueIsDefault = options.DefaultAttributeResponseBehavior.OmitAttributeIfValueIsDefault;
            _options = options;
        }

        /// <inheritdoc/>
        public bool OmitAttributeIfValueIsDefault { get; private set; }

        public bool IsEnabled(DisableQueryAttribute disableQueryAttribute)
        {
            return _options.DefaultAttributeResponseBehavior.AllowQueryStringOverride &&
                   !disableQueryAttribute.ContainsParameter(StandardQueryStringParameters.OmitDefault);
        }

        /// <inheritdoc/>
        public bool CanParse(string parameterName)
        {
            return parameterName == "omitDefault";
        }

        /// <inheritdoc/>
        public virtual void Parse(string parameterName, StringValues parameterValue)
        {
            if (!bool.TryParse(parameterValue, out var omitAttributeIfValueIsDefault))
            {
                throw new InvalidQueryStringParameterException(parameterName,
                    "The specified query string value must be 'true' or 'false'.",
                    $"The value '{parameterValue}' for parameter '{parameterName}' is not a valid boolean value.");
            }

            OmitAttributeIfValueIsDefault = omitAttributeIfValueIsDefault;
        }
    }
}
