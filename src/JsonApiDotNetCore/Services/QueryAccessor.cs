using System;
using System.Linq;
using JsonApiDotNetCore.Internal;
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

    public class QueryAccessor : IQueryAccessor
    {
        private readonly IJsonApiContext _jsonApiContext;
        private readonly ILogger<QueryAccessor> _logger;

        public QueryAccessor(
            IJsonApiContext jsonApiContext,
            ILogger<QueryAccessor> logger)
        {
            _jsonApiContext = jsonApiContext;
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
                _logger.LogInformation($"'{value}' is not a valid '{typeof(T).Name}' value for query parameter {key}");
                return false;
            }
        }

        private string GetFilterValue(string key) => _jsonApiContext.QuerySet
            .Filters
            .FirstOrDefault(f => f.Key == key)
            ?.Value;
    }
}