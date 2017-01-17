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
            _logger.LogError(context.Exception.Message);

            var jsonApiException = (JsonApiException)context.Exception;
            if(jsonApiException != null)
            {
                var error = jsonApiException.GetError();
                var result = new ObjectResult(error);
                result.StatusCode = Convert.ToInt16(error.Status);
                context.Result = result; 
            }
        }
    }
}
