using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace JsonApiDotNetCore.Formatters
{
    public interface IJsonApiReader
    {
        Task<InputFormatterResult> ReadAsync(InputFormatterContext context);
    }
}