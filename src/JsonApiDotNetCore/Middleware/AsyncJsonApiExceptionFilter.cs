using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware;

/// <inheritdoc cref="IAsyncJsonApiExceptionFilter" />
[PublicAPI]
public sealed class AsyncJsonApiExceptionFilter : IAsyncJsonApiExceptionFilter
{
    private readonly IExceptionHandler _exceptionHandler;

    public AsyncJsonApiExceptionFilter(IExceptionHandler exceptionHandler)
    {
        ArgumentNullException.ThrowIfNull(exceptionHandler);

        _exceptionHandler = exceptionHandler;
    }

    /// <inheritdoc />
    public Task OnExceptionAsync(ExceptionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

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
