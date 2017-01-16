using JsonApiDotNetCore.Internal;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Services
{
    public interface IJsonApiContext
    {
        void ApplyContext(HttpContext context);
        IContextGraph ContextGraph { get; set; }
        ContextEntity RequestEntity { get; set; }
        string BasePath { get; set; }
        IQueryCollection Query { get; set; }
    }
}
