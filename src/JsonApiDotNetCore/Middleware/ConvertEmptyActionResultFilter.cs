using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace JsonApiDotNetCore.Middleware
{
    /// <inheritdoc />
    public sealed class ConvertEmptyActionResultFilter : IConvertEmptyActionResultFilter
    {
        public void OnResultExecuted(ResultExecutedContext context) { /* noop */ }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!context.HttpContext.IsJsonApiRequest())
            {
                return;
            }

            switch (context.Result)
            {
                case ObjectResult objectResult when objectResult.Value != null:
                    return;
                case IStatusCodeActionResult statusCodeResult:
                    context.Result = new ObjectResult(null) { StatusCode = statusCodeResult.StatusCode };
                    break;
            }
        }
    }
}
