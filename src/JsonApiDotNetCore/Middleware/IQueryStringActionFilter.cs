using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Entry point for processing all query string parameters.
    /// </summary>
    public interface IQueryStringActionFilter : IActionFilter { }
}
