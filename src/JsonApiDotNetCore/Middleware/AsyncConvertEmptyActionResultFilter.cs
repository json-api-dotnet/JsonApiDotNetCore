using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace JsonApiDotNetCore.Middleware
{
    /// <inheritdoc />
    public sealed class AsyncConvertEmptyActionResultFilter : IAsyncConvertEmptyActionResultFilter
    {
        /// <inheritdoc />
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (next == null) throw new ArgumentNullException(nameof(next));

            if (context.HttpContext.IsJsonApiRequest())
            {
                if (!(context.Result is ObjectResult objectResult) || objectResult.Value == null)
                {
                    if (context.Result is IStatusCodeActionResult statusCodeResult)
                    {
                        context.Result = new ObjectResult(null) {StatusCode = statusCodeResult.StatusCode};
                    }
                }
            }

            await next();
        }
    }
}
