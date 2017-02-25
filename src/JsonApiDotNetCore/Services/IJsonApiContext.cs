using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Query;

namespace JsonApiDotNetCore.Services
{
    public interface IJsonApiContext
    {
        JsonApiOptions Options { get; set; }
        IJsonApiContext ApplyContext<T>();
        IContextGraph ContextGraph { get; set; }
        ContextEntity RequestEntity { get; set; }
        string BasePath { get; set; }
        QuerySet QuerySet { get; set; }
        bool IsRelationshipData { get; set; }
        List<string> IncludedRelationships { get; set; }
        int TotalRecords { get; set; }
    }
}
