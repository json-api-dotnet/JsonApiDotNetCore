using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Serialization.Response
{
    /// <summary>
    /// Serializes ASP.NET models into the outgoing JSON:API response body.
    /// </summary>
    [PublicAPI]
    public interface IJsonApiWriter
    {
        /// <summary>
        /// Writes an object to the response body.
        /// </summary>
        Task WriteAsync(object? model, HttpContext httpContext);
    }
}
