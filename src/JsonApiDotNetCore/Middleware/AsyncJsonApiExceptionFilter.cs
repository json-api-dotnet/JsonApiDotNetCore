#nullable disable

using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    /// <inheritdoc />
    [PublicAPI]
    public sealed class AsyncJsonApiExceptionFilter : IAsyncJsonApiExceptionFilter
    {
        private readonly IExceptionHandler _exceptionHandler;

        public AsyncJsonApiExceptionFilter(IExceptionHandler exceptionHandler)
        {
            ArgumentGuard.NotNull(exceptionHandler, nameof(exceptionHandler));

            _exceptionHandler = exceptionHandler;
        }

        /// <inheritdoc />
        public Task OnExceptionAsync(ExceptionContext context)
        {
            ArgumentGuard.NotNull(context, nameof(context));

            if (context.HttpContext.IsJsonApiRequest())
            {
                IReadOnlyList<ErrorObject> errors = _exceptionHandler.HandleException(context.Exception);

                context.Result = new ObjectResult(errors)
                {
                    StatusCode = (int)ErrorObject.GetResponseStatusCode(errors)
                };
            }

            return Task.CompletedTask;
        }
    }
}
