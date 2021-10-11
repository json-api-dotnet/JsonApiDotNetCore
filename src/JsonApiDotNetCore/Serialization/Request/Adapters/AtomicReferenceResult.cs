#nullable disable

using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Serialization.Request.Adapters
{
    /// <summary>
    /// The result of validating and converting "ref" in an entry of an atomic:operations request.
    /// </summary>
    [PublicAPI]
    public sealed class AtomicReferenceResult
    {
        public IIdentifiable Resource { get; }
        public ResourceType ResourceType { get; }
        public RelationshipAttribute Relationship { get; }

        public AtomicReferenceResult(IIdentifiable resource, ResourceType resourceType, RelationshipAttribute relationship)
        {
            ArgumentGuard.NotNull(resource, nameof(resource));
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            Resource = resource;
            ResourceType = resourceType;
            Relationship = relationship;
        }
    }
}
