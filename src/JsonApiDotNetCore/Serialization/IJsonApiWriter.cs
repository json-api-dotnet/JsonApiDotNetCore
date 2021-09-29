using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Serializes models into the outgoing JSON response body.
    /// </summary>
    [PublicAPI]
    public interface IJsonApiWriter
    {
        Task WriteAsync(OutputFormatterWriteContext context);
    }
}
