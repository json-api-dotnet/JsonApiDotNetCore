using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Global exception filter that wraps any thrown error with a JsonApiException.
    /// </summary>
    public sealed class DefaultExceptionFilter : ActionFilterAttribute, IExceptionFilter
    {
        private readonly ILogger _logger;

        public DefaultExceptionFilter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DefaultExceptionFilter>();
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(new EventId(), context.Exception, "An unhandled exception occurred during the request");

            var jsonApiException = JsonApiExceptionFactory.GetException(context.Exception);

            context.Result = new ObjectResult(new ErrorDocument(jsonApiException.Error))
            {
                StatusCode = (int) jsonApiException.Error.Status
            };
        }
    }
}
