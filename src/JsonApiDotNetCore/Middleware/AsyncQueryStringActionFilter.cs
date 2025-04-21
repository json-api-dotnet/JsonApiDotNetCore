using System.Reflection;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.QueryStrings;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware;

/// <inheritdoc cref="IAsyncQueryStringActionFilter" />
public sealed class AsyncQueryStringActionFilter : IAsyncQueryStringActionFilter
{
    private readonly IQueryStringReader _queryStringReader;

    public AsyncQueryStringActionFilter(IQueryStringReader queryStringReader)
    {
        ArgumentNullException.ThrowIfNull(queryStringReader);

        _queryStringReader = queryStringReader;
    }

    /// <inheritdoc />
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        if (context.HttpContext.IsJsonApiRequest())
        {
            var disableQueryStringAttribute = context.Controller.GetType().GetCustomAttribute<DisableQueryStringAttribute>(true);
            _queryStringReader.ReadAll(disableQueryStringAttribute);
        }

        await next();
    }
}
