using Microsoft.AspNetCore.Mvc.Formatters;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Application-wide entry point for writing json:api response bodies.
    /// </summary>
    public interface IJsonApiOutputFormatter : IOutputFormatter { }
}
