using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Global exception filter that wraps any thrown error with a JsonApiException.
    /// </summary>
    public interface IJsonApiExceptionFilter : IExceptionFilter { }
}
