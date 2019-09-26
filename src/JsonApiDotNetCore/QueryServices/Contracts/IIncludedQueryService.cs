using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IIncludedQueryService
    {
        List<List<RelationshipAttribute>> Get();
    }

    public interface IInternalIncludedQueryService
    {
        void Register(List<RelationshipAttribute> inclusionChain);
    }
}