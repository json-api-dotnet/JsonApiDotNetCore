using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Query;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Services
{
    /// <inheritdoc/>
    public class QueryParameterParser : IQueryParameterParser
    {
        private readonly IJsonApiOptions _options;
        private readonly IEnumerable<IQueryParameterService> _queryServices;

        public QueryParameterParser(IJsonApiOptions options, IEnumerable<IQueryParameterService> queryServices)
        {
            _options = options;
            _queryServices = queryServices;
        }

        /// <summary>
        /// For a query parameter in <paramref name="query"/>, calls
        /// the <see cref="IQueryParameterService.Parse(KeyValuePair{string, Microsoft.Extensions.Primitives.StringValues})"/>
        /// method of the corresponding service.
        /// </summary>
        public virtual void Parse(IQueryCollection query, DisableQueryAttribute disabled)
        {
            var disabledQuery = disabled?.QueryParams;

            foreach (var pair in query)
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
                    throw new JsonApiException(400, $"{pair} is not a valid query.");
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
