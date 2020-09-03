using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    /// <inheritdoc />
    public class AsyncJsonApiExceptionFilter : IAsyncJsonApiExceptionFilter
    {
        private readonly IExceptionHandler _exceptionHandler;

        public AsyncJsonApiExceptionFilter(IExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public Task OnExceptionAsync(ExceptionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (context.HttpContext.IsJsonApiRequest())
            {
                var errorDocument = _exceptionHandler.HandleException(context.Exception);

                context.Result = new ObjectResult(errorDocument)
                {
                    StatusCode = (int) errorDocument.GetErrorStatusCode()
                };
            }

            return Task.CompletedTask;
        }
    }
}
