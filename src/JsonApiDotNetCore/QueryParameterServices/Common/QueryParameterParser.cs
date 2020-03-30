using System;
using System.Collections.Generic;
using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal;
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

        /// <summary>
        /// For a parameter in the query string of the request URL, calls
        /// the <see cref="IQueryParameterService.Parse(KeyValuePair{string, Microsoft.Extensions.Primitives.StringValues})"/>
        /// method of the corresponding service.
        /// </summary>
        public virtual void Parse(DisableQueryAttribute disabled)
        {
            var disabledQuery = disabled?.QueryParams;

            foreach (var pair in _queryStringAccessor.Query)
            {
                bool parsed = false;
                foreach (var service in _queryServices)
                {
                    if (pair.Key.ToLower().StartsWith(service.Name, StringComparison.Ordinal))
                    {
                        if (disabledQuery == null || !IsDisabled(disabledQuery, service))
                            service.Parse(pair);
                        parsed = true;
                        break;
                    }
                }
                if (parsed)
                    continue;

                if (!_options.AllowCustomQueryParameters)
                    throw new JsonApiException(HttpStatusCode.BadRequest, $"{pair} is not a valid query.");
            }
        }

        private bool IsDisabled(string disabledQuery, IQueryParameterService targetsService)
        {
            if (disabledQuery == QueryParams.All.ToString("G").ToLower())
                return true;

            if (disabledQuery == targetsService.Name)
                return true;

            return false;
        }
    }
}
