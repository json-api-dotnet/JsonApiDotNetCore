using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace JsonApiDotNetCore.Formatters
{
    public interface IJsonApiWriter
    {
        Task WriteAsync(OutputFormatterWriteContext context);
    }
}