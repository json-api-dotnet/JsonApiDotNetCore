using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Application-wide exception filter that invokes <see cref="IExceptionHandler" /> for JSON:API requests.
    /// </summary>
    [PublicAPI]
    public interface IAsyncJsonApiExceptionFilter : IAsyncExceptionFilter
    {
    }
}
