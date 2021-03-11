using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Application-wide entry point for writing JSON:API response bodies.
    /// </summary>
    [PublicAPI]
    public interface IJsonApiOutputFormatter : IOutputFormatter
    {
    }
}
