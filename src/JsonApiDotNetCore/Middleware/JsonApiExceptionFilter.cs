using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Global exception filter that wraps any thrown error with a JsonApiException.
    /// </summary>
    public class JsonApiExceptionFilter : ActionFilterAttribute, IJsonApiExceptionFilter
    {
        private readonly IExceptionHandler _exceptionHandler;

        public JsonApiExceptionFilter(IExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        public void OnException(ExceptionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            
            if (context.HttpContext.IsJsonApiRequest())
            {
                return;
            }
            
            var errorDocument = _exceptionHandler.HandleException(context.Exception);

            context.Result = new ObjectResult(errorDocument)
            {
                StatusCode = (int) errorDocument.GetErrorStatusCode()
            };
        }
    }
}
