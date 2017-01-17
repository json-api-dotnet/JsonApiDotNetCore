using System.Collections.Generic;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Query;

namespace JsonApiDotNetCore.Services
{
    public interface IJsonApiContext
    {
        IJsonApiContext ApplyContext<T>();
        IContextGraph ContextGraph { get; set; }
        ContextEntity RequestEntity { get; set; }
        string BasePath { get; set; }
        QuerySet QuerySet { get; set; }
        bool IsRelationshipData { get; set; }
        List<string> IncludedRelationships { get; set; }
    }
}
