using System;
using JsonApiDotNetCore.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Formatters
{
    public class JsonApiExceptionFilter : ActionFilterAttribute, IExceptionFilter
    {
        private readonly ILogger _logger;
        
        public JsonApiExceptionFilter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<JsonApiExceptionFilter>();
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(new EventId(), context.Exception, "An unhandled exception occurred during the request");

            var jsonApiException = context.Exception as JsonApiException;
            
            if(jsonApiException == null)
                jsonApiException = new JsonApiException("500", context.Exception.Message);

            var error = jsonApiException.GetError();
            var result = new ObjectResult(error);
            result.StatusCode = Convert.ToInt16(error.Status);
            context.Result = result;         
        }
    }
}
