using Microsoft.AspNetCore.Mvc.Formatters;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Application-wide entry point for writing JSON:API response bodies.
    /// </summary>
    public interface IJsonApiOutputFormatter : IOutputFormatter { }
}
