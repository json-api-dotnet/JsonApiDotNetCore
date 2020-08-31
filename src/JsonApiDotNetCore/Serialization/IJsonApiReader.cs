using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// The deserializer of the body, used in ASP.NET Core internally
    /// to process `FromBody`.
    /// </summary>
    public interface IJsonApiReader
    {
        Task<InputFormatterResult> ReadAsync(InputFormatterContext context);
    }
}
