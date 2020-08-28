using JsonApiDotNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace JsonApiDotNetCore.Middleware
{ 
    /// <summary>
    /// Converts action result without parameters into action result with null parameter.
    /// For example: return NotFound() -> return NotFound(null)
    /// This ensures our formatter is invoked, where we'll build a json:api compliant response.
    /// For details, see: https://github.com/dotnet/aspnetcore/issues/16969
    /// </summary>
    public sealed class ConvertEmptyActionResultFilter : IAlwaysRunResultFilter
    {
        public void OnResultExecuted(ResultExecutedContext context) { /* noop */ }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (!context.HttpContext.IsJsonApiRequest())
            {
                return;
            }

            switch (context.Result)
            {
                case ObjectResult objectResult when objectResult.Value != null:
                    return;
                case IStatusCodeActionResult statusCodeResult:
                    context.Result = new ObjectResult(null) {StatusCode = statusCodeResult.StatusCode};
                    break;
            }
        }
    }
}
