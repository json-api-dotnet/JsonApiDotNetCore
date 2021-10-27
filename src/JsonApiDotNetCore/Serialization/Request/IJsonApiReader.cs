using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Serialization.Request
{
    /// <summary>
    /// Deserializes the incoming JSON:API request body and converts it to models, which are passed to controller actions by ASP.NET Core on `FromBody`
    /// parameters.
    /// </summary>
    [PublicAPI]
    public interface IJsonApiReader
    {
        /// <summary>
        /// Reads an object from the request body.
        /// </summary>
        Task<object> ReadAsync(HttpRequest httpRequest);
    }
}
