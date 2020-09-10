using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Application-wide entry point for processing json:api request query strings.
    /// </summary>
    public interface IAsyncQueryStringActionFilter : IAsyncActionFilter { }
}
