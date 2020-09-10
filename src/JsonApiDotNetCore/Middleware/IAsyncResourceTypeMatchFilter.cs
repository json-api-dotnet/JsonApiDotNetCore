using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Verifies the incoming resource type in json:api request body matches the resource type at the current endpoint URL.
    /// </summary>
    public interface IAsyncResourceTypeMatchFilter : IAsyncActionFilter { }
}
