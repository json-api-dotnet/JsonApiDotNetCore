using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace JsonApiDotNetCore.Middleware
{
    public sealed class ConvertEmptyActionResultFilter : IAlwaysRunResultFilter
    {
        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (!context.HttpContext.IsJsonApiRequest())
            {
                return;
            }

            if (context.Result is ObjectResult objectResult && objectResult.Value != null)
            {
                return;
            }

            // Convert action result without parameters into action result with null parameter.
            // For example: return NotFound() -> return NotFound(null)
            // This ensures our formatter is invoked, where we'll build a json:api compliant response.
            // For details, see: https://github.com/dotnet/aspnetcore/issues/16969
            if (context.Result is IStatusCodeActionResult statusCodeResult)
            {
                context.Result = new ObjectResult(null) {StatusCode = statusCodeResult.StatusCode};
            }
        }
    }
}
