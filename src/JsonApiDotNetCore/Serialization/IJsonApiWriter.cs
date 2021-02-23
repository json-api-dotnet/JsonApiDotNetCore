using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace JsonApiDotNetCore.Serialization
{
    public interface IJsonApiWriter
    {
        Task WriteAsync(OutputFormatterWriteContext context);
    }
}
