using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Building
{
    public interface IIncludedResourceObjectBuilder
    {
        /// <summary>
        /// Gets the list of resource objects representing the included resources.
        /// </summary>
        IList<ResourceObject> Build();

        /// <summary>
        /// Extracts the included resources from <paramref name="rootResource" /> using the (arbitrarily deeply nested) included relationships in
        /// <paramref name="inclusionChain" />.
        /// </summary>
        void IncludeRelationshipChain(IReadOnlyCollection<RelationshipAttribute> inclusionChain, IIdentifiable rootResource);
    }
}
