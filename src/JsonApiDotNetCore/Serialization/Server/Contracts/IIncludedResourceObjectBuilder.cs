using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCore.Serialization.Server.Builders
{
    public interface IIncludedResourceObjectBuilder
    {
        /// <summary>
        /// Gets the list of resource objects representing the included resources
        /// </summary>
        List<ResourceObject> Build();
        /// <summary>
        /// Extracts the included resources from <paramref name="rootResource"/> using the
        /// (arbitrarily deeply nested) included relationships in <paramref name="inclusionChain"/>.
        /// </summary>
        void IncludeRelationshipChain(List<RelationshipAttribute> inclusionChain, IIdentifiable rootResource);
    }
}
