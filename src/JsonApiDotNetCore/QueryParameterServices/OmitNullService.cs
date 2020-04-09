using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Exceptions;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Query
{
    /// <inheritdoc/>
    public class OmitNullService : QueryParameterService, IOmitNullService
    {
        private readonly IJsonApiOptions _options;

        public OmitNullService(IJsonApiOptions options)
        {
            OmitAttributeIfValueIsNull = options.SerializerOmitAttributeIfValueIsNull;
            _options = options;
        }

        /// <inheritdoc/>
        public bool OmitAttributeIfValueIsNull { get; private set; }

        /// <inheritdoc/>
        public bool IsEnabled(DisableQueryAttribute disableQueryAttribute)
        {
            return _options.AllowOmitNullQueryStringOverride && 
                   !disableQueryAttribute.ContainsParameter(StandardQueryStringParameters.OmitNull);
        }

        /// <inheritdoc/>
        public bool CanParse(string parameterName)
        {
            return parameterName == "omitNull";
        }

        /// <inheritdoc/>
        public virtual void Parse(string parameterName, StringValues parameterValue)
        {
            if (!bool.TryParse(parameterValue, out var omitAttributeIfValueIsNull))
            {
                throw new InvalidQueryStringParameterException(parameterName,
                    "The specified query string value must be 'true' or 'false'.",
                    $"The value '{parameterValue}' for parameter '{parameterName}' is not a valid boolean value.");
            }

            OmitAttributeIfValueIsNull = omitAttributeIfValueIsNull;
        }
    }
}
