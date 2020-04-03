using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Query;
using JsonApiDotNetCore.QueryParameterServices.Common;

namespace JsonApiDotNetCore.Services
{
    /// <inheritdoc/>
    public class QueryParameterParser : IQueryParameterParser
    {
        private readonly IJsonApiOptions _options;
        private readonly IRequestQueryStringAccessor _queryStringAccessor;
        private readonly IEnumerable<IQueryParameterService> _queryServices;

        public QueryParameterParser(IJsonApiOptions options, IRequestQueryStringAccessor queryStringAccessor, IEnumerable<IQueryParameterService> queryServices)
        {
            _options = options;
            _queryStringAccessor = queryStringAccessor;
            _queryServices = queryServices;
        }

        /// <inheritdoc/>
        public virtual void Parse(DisableQueryAttribute disableQueryAttribute)
        {
            disableQueryAttribute ??= DisableQueryAttribute.Empty;

            foreach (var pair in _queryStringAccessor.Query)
            {
                if (string.IsNullOrWhiteSpace(pair.Value))
                {
                    throw new InvalidQueryStringParameterException(pair.Key, "Missing query string parameter value.",
                        $"Missing value for '{pair.Key}' query string parameter.");
                }

                var service = _queryServices.FirstOrDefault(s => s.CanParse(pair.Key));
                if (service != null)
                {
                    if (!service.IsEnabled(disableQueryAttribute))
                    {
                        throw new InvalidQueryStringParameterException(pair.Key,
                            "Usage of one or more query string parameters is not allowed at the requested endpoint.",
                            $"The parameter '{pair.Key}' cannot be used at this endpoint.");
                    }

                    service.Parse(pair.Key, pair.Value);
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
