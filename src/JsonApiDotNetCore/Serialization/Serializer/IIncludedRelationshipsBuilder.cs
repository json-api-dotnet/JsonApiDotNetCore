using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Builders
{
    public interface IIncludedRelationshipsBuilder
    {
        List<ResourceObject> Build();
        void IncludeRelationshipChain(List<RelationshipAttribute> inclusionChain, IIdentifiable rootEntity);
    }
}