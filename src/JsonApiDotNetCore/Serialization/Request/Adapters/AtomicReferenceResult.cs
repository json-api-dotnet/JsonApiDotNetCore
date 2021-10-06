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
        public ResourceContext ResourceContext { get; }
        public RelationshipAttribute Relationship { get; }

        public AtomicReferenceResult(IIdentifiable resource, ResourceContext resourceContext, RelationshipAttribute relationship)
        {
            ArgumentGuard.NotNull(resource, nameof(resource));
            ArgumentGuard.NotNull(resourceContext, nameof(resourceContext));

            Resource = resource;
            ResourceContext = resourceContext;
            Relationship = relationship;
        }
    }
}
