using JsonApiDotNetCore.Internal;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Services
{
    public interface IJsonApiContext
    {
        IJsonApiContext ApplyContext<T>();
        IContextGraph ContextGraph { get; set; }
        ContextEntity RequestEntity { get; set; }
        string BasePath { get; set; }
        IQueryCollection Query { get; set; }
        bool IsRelationshipData { get; set; }
    }
}
