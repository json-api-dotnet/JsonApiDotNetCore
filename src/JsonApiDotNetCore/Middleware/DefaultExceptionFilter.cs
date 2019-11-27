using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Managers.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Global exception filter that wraps any thrown error with a JsonApiException.
    /// </summary>
    public class DefaultExceptionFilter : ActionFilterAttribute, IExceptionFilter
    {
        private readonly ICurrentRequest _currentRequest;
        private readonly ILogger _logger;

        public DefaultExceptionFilter(ILoggerFactory loggerFactory, ICurrentRequest currentRequest)
        {
            _currentRequest = currentRequest;
            _logger = loggerFactory.CreateLogger<DefaultExceptionFilter>();
        }

        public void OnException(ExceptionContext context)
        {
            _logger?.LogError(new EventId(), context.Exception, "An unhandled exception occurred during the request");
            var jsonApiException = JsonApiExceptionFactory.GetException(context.Exception);
            var error = jsonApiException.GetError();
            var result = new ObjectResult(error)
            {
                StatusCode = jsonApiException.GetStatusCode()
            };
            context.Result = result;
        }
    }
}
