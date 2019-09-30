using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Serialization.Serializer.Contracts
{
    public interface IIncludedRelationshipsBuilder
    {
        /// <summary>
        /// Gets the list of resource objects representing the included entities
        /// </summary>
        List<ResourceObject> Build();
        /// <summary>
        /// Extracts the included entities from <paramref name="rootEntity"/> using the
        /// (arbitrarly deeply nested) included relationships in <paramref name="inclusionChain"/>.
        /// </summary>
        void IncludeRelationshipChain(List<RelationshipAttribute> inclusionChain, IIdentifiable rootEntity);
    }
}