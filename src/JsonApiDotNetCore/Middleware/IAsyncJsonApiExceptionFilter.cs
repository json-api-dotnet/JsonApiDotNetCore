using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Application-wide exception filter that invokes <see cref="IExceptionHandler"/> for json:api requests.
    /// </summary>
    public interface IAsyncJsonApiExceptionFilter : IAsyncExceptionFilter { }
}
