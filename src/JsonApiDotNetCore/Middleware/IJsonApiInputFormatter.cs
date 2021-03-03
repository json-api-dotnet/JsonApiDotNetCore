using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Application-wide entry point for reading JSON:API request bodies.
    /// </summary>
    [PublicAPI]
    public interface IJsonApiInputFormatter : IInputFormatter
    {
    }
}
