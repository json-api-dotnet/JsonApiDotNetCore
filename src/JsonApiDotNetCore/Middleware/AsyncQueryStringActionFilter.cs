using System.Reflection;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.QueryStrings;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware;

/// <inheritdoc />
public sealed class AsyncQueryStringActionFilter : IAsyncQueryStringActionFilter
{
    private readonly IQueryStringReader _queryStringReader;

    public AsyncQueryStringActionFilter(IQueryStringReader queryStringReader)
    {
        ArgumentGuard.NotNull(queryStringReader);

        _queryStringReader = queryStringReader;
    }

    /// <inheritdoc />
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentGuard.NotNull(context);
        ArgumentGuard.NotNull(next);

        if (context.HttpContext.IsJsonApiRequest())
        {
            var disableQueryStringAttribute = context.Controller.GetType().GetCustomAttribute<DisableQueryStringAttribute>(true);
            _queryStringReader.ReadAll(disableQueryStringAttribute);
        }

        await next();
    }
}
