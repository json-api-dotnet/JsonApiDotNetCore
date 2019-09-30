using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.QueryServices.Contracts
{

    public interface IIncludedQueryService
    {
        /// <summary>
        /// Gets the list of included relationships chains for the current request.
        /// </summary>
        List<List<RelationshipAttribute>> Get();
    }

    public interface IInternalIncludedQueryService
    {
        void Register(List<RelationshipAttribute> inclusionChain);
    }
}