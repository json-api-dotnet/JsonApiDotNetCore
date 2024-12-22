using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace JsonApiDotNetCore.Middleware;

/// <inheritdoc cref="IAsyncConvertEmptyActionResultFilter" />
public sealed class AsyncConvertEmptyActionResultFilter : IAsyncConvertEmptyActionResultFilter
{
    /// <inheritdoc />
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        if (context.HttpContext.IsJsonApiRequest())
        {
            if (context.Result is not ObjectResult objectResult || objectResult.Value == null)
            {
                if (context.Result is IStatusCodeActionResult statusCodeResult)
                {
                    context.Result = new ObjectResult(null)
                    {
                        StatusCode = statusCodeResult.StatusCode
                    };
                }
            }
        }

        await next();
    }
}
