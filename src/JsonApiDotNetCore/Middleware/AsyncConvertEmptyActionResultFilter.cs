using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace JsonApiDotNetCore.Middleware;

/// <inheritdoc />
public sealed class AsyncConvertEmptyActionResultFilter : IAsyncConvertEmptyActionResultFilter
{
    /// <inheritdoc />
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        ArgumentGuard.NotNull(context);
        ArgumentGuard.NotNull(next);

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
