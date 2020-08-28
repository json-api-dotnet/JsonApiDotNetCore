using JsonApiDotNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    public class JsonApiExceptionFilter : ActionFilterAttribute, IJsonApiExceptionFilter
    {
        private readonly IExceptionHandler _exceptionHandler;

        public JsonApiExceptionFilter(IExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler;
        }

        public void OnException(ExceptionContext context)
        {
            if (!context.HttpContext.IsJsonApiRequest())
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
