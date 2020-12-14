using Microsoft.AspNetCore.Mvc.Formatters;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Application-wide entry point for reading JSON:API request bodies.
    /// </summary>
    public interface IJsonApiInputFormatter : IInputFormatter { }
}
