using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace JsonApiDotNetCore.Serialization
{
    [PublicAPI]
    public interface IJsonApiWriter
    {
        Task WriteAsync(OutputFormatterWriteContext context);
    }
}
