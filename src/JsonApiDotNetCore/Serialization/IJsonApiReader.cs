using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Deserializes the incoming JSON request body and converts it to models, which are passed to controller actions by ASP.NET Core on `FromBody`
    /// parameters.
    /// </summary>
    [PublicAPI]
    public interface IJsonApiReader
    {
        /// <summary>
        /// Reads an object from the request body.
        /// </summary>
        Task<InputFormatterResult> ReadAsync(HttpRequest httpRequest);
    }
}
