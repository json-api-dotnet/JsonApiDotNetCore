using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.QueryStrings;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Internal.QueryStrings
{
    /// <inheritdoc/>
    public class QueryStringReader : IQueryStringReader
    {
        private readonly IJsonApiOptions _options;
        private readonly IRequestQueryStringAccessor _queryStringAccessor;
        private readonly IEnumerable<IQueryStringParameterReader> _parameterReaders;
        private readonly ILogger<QueryStringReader> _logger;

        public QueryStringReader(IJsonApiOptions options, IRequestQueryStringAccessor queryStringAccessor,
            IEnumerable<IQueryStringParameterReader> parameterReaders, ILoggerFactory loggerFactory)
        {
            _options = options;
            _queryStringAccessor = queryStringAccessor;
            _parameterReaders = parameterReaders;

            _logger = loggerFactory.CreateLogger<QueryStringReader>();
        }

        /// <inheritdoc/>
        public virtual void ReadAll(DisableQueryAttribute disableQueryAttribute)
        {
            disableQueryAttribute ??= DisableQueryAttribute.Empty;

            foreach (var (parameterName, parameterValue) in _queryStringAccessor.Query)
            {
                if (string.IsNullOrEmpty(parameterValue))
                {
                    throw new InvalidQueryStringParameterException(parameterName,
                        "Missing query string parameter value.",
                        $"Missing value for '{parameterName}' query string parameter.");
                }

                var reader = _parameterReaders.FirstOrDefault(r => r.CanRead(parameterName));
                if (reader != null)
                {
                    _logger.LogDebug(
                        $"Query string parameter '{parameterName}' with value '{parameterValue}' was accepted by {reader.GetType().Name}.");

                    if (!reader.IsEnabled(disableQueryAttribute))
                    {
                        throw new InvalidQueryStringParameterException(parameterName,
                            "Usage of one or more query string parameters is not allowed at the requested endpoint.",
                            $"The parameter '{parameterName}' cannot be used at this endpoint.");
                    }

                    reader.Read(parameterName, parameterValue);
                    _logger.LogDebug($"Query string parameter '{parameterName}' was successfully read.");
                }
                else if (!_options.AllowUnknownQueryStringParameters)
                {
                    throw new InvalidQueryStringParameterException(parameterName, "Unknown query string parameter.",
                        $"Query string parameter '{parameterName}' is unknown. " +
                        $"Set '{nameof(IJsonApiOptions.AllowUnknownQueryStringParameters)}' to 'true' in options to ignore unknown parameters.");
                }
            }
        }
    }
}
