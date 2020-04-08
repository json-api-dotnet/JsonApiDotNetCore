using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Query;
using JsonApiDotNetCore.QueryParameterServices.Common;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Services
{
    /// <inheritdoc/>
    public class QueryParameterParser : IQueryParameterParser
    {
        private readonly IJsonApiOptions _options;
        private readonly IRequestQueryStringAccessor _queryStringAccessor;
        private readonly IEnumerable<IQueryParameterService> _queryServices;
        private ILogger<QueryParameterParser> _logger;

        public QueryParameterParser(IJsonApiOptions options, IRequestQueryStringAccessor queryStringAccessor, IEnumerable<IQueryParameterService> queryServices, ILoggerFactory loggerFactory)
        {
            _options = options;
            _queryStringAccessor = queryStringAccessor;
            _queryServices = queryServices;

            _logger = loggerFactory.CreateLogger<QueryParameterParser>();
        }

        /// <inheritdoc/>
        public virtual void Parse(DisableQueryAttribute disableQueryAttribute)
        {
            disableQueryAttribute ??= DisableQueryAttribute.Empty;

            foreach (var pair in _queryStringAccessor.Query)
            {
                if (string.IsNullOrEmpty(pair.Value))
                {
                    throw new InvalidQueryStringParameterException(pair.Key, "Missing query string parameter value.",
                        $"Missing value for '{pair.Key}' query string parameter.");
                }

                var service = _queryServices.FirstOrDefault(s => s.CanParse(pair.Key));
                if (service != null)
                {
                    _logger.LogDebug($"Query string parameter '{pair.Key}' with value '{pair.Value}' was accepted by {service.GetType().Name}.");

                    if (!service.IsEnabled(disableQueryAttribute))
                    {
                        throw new InvalidQueryStringParameterException(pair.Key,
                            "Usage of one or more query string parameters is not allowed at the requested endpoint.",
                            $"The parameter '{pair.Key}' cannot be used at this endpoint.");
                    }

                    service.Parse(pair.Key, pair.Value);
                    _logger.LogDebug($"Query string parameter '{pair.Key}' was successfully parsed.");
                }
                else if (!_options.AllowCustomQueryStringParameters)
                {
                    throw new InvalidQueryStringParameterException(pair.Key, "Unknown query string parameter.",
                        $"Query string parameter '{pair.Key}' is unknown. Set '{nameof(IJsonApiOptions.AllowCustomQueryStringParameters)}' to 'true' in options to ignore unknown parameters.");
                }
            }
        }
    }
}
