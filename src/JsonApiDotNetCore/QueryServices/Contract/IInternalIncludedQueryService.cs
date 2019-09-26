using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IInternalIncludedQueryService
    {
        void Register(List<RelationshipAttribute> inclusionChain);
    }
}