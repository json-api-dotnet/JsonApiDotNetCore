using System;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Managers.Contracts;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Services
{
    public interface IQueryAccessor
    {
        bool TryGetValue<T>(string key, out T value);

        /// <summary>
        /// Gets the query value and throws a if it is not present.
        /// If the exception is not caught, the middleware will return an HTTP 422 response.
        /// </summary>
        /// <exception cref="JsonApiException" />
        T GetRequired<T>(string key);
    }

    /// <summary>
    /// Accessing queries
    /// </summary>
    public class QueryAccessor : IQueryAccessor
    {
        private readonly IRequestContext _requestManager;
        private readonly ILogger<QueryAccessor> _logger;

        /// <summary>
        /// Creates an instance which can be used to access the qury
        /// </summary>
        /// <param name="requestManager"></param>
        /// <param name="logger"></param>
        public QueryAccessor(
            IRequestContext requestManager,
            ILogger<QueryAccessor> logger)
        {
            _requestManager = requestManager;
            _logger = logger;
        }

  
        public T GetRequired<T>(string key)
        {
            if (TryGetValue<T>(key, out T result) == false)
                throw new JsonApiException(422, $"'{key}' is not a valid '{typeof(T).Name}' value for query parameter {key}");

            return result;
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            value = default(T);

            var stringValue = GetFilterValue(key);
            if (stringValue == null)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation($"'{key}' was not found in the query collection");
                }

                return false;
            }

            try
            {
                value = TypeHelper.ConvertType<T>(stringValue);
                return true;
            }
            catch (FormatException)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation(
                        $"'{value}' is not a valid '{typeof(T).Name}' value for query parameter {key}");
                }

                return false;
            }
        }

        private string GetFilterValue(string key) {
            var publicValue = _requestManager.QuerySet.Filters
                .FirstOrDefault(f => string.Equals(f.Attribute, key, StringComparison.OrdinalIgnoreCase))?.Value;
            
            if(publicValue != null) 
                return publicValue;
            
            var internalValue = _requestManager.QuerySet.Filters
                .FirstOrDefault(f => string.Equals(f.Attribute, key, StringComparison.OrdinalIgnoreCase))?.Value;
            
            if(internalValue != null) {
                _logger.LogWarning("Locating filters by the internal propterty name is deprecated. You should use the public attribute name instead.");
                return publicValue;
            }

            return null;
        }
    }
}
