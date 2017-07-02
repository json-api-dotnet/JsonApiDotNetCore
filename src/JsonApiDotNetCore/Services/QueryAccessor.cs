using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Services
{
    public class QueryAccessor : IQueryAccessor
    {
        private readonly IJsonApiContext _jsonApiContext;
        private readonly ILogger<QueryAccessor> _logger;

        public QueryAccessor(
            IJsonApiContext jsonApiContext, 
            ILoggerFactory loggerFactory)
        {
            _jsonApiContext = jsonApiContext;
            _logger = loggerFactory.CreateLogger<QueryAccessor>();
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            value = default(T);

            var stringValue = GetFilterValue(key);            
            if(stringValue == null)
            {
                _logger.LogInformation($"'{key}' was not found in the query collection");
                return false;
            }
                
            try
            {
                value = TypeHelper.ConvertType<T>(stringValue);
                return true;
            }
            catch (FormatException)
            {
                _logger.LogInformation($"'{value}' is not a valid guid value for query parameter {key}");
                return false;
            }
        }

        private string GetFilterValue(string key) => _jsonApiContext.QuerySet
            .Filters
            .FirstOrDefault(f => f.Key == key)
            ?.Value;
    }
}