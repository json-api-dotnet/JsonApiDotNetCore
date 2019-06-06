using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace JsonApiDotNetCore.Formatters
{
    /// <summary>
    /// The deserializer of the body, used in .NET core internally
    /// to process `FromBody`
    /// </summary>
    public interface IJsonApiReader
    {
        Task<InputFormatterResult> ReadAsync(InputFormatterContext context);
    }
}
