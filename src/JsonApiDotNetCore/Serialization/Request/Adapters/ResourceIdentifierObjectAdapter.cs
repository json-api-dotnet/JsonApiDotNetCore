using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters
{
    /// <inheritdoc cref="IResourceIdentifierObjectAdapter" />
    public sealed class ResourceIdentifierObjectAdapter : ResourceIdentityAdapter, IResourceIdentifierObjectAdapter
    {
        public ResourceIdentifierObjectAdapter(IResourceGraph resourceGraph, IResourceFactory resourceFactory)
            : base(resourceGraph, resourceFactory)
        {
        }

        /// <inheritdoc />
        public IIdentifiable Convert(ResourceIdentifierObject resourceIdentifierObject, ResourceIdentityRequirements requirements, RequestAdapterState state)
        {
            ArgumentGuard.NotNull(resourceIdentifierObject, nameof(resourceIdentifierObject));
            ArgumentGuard.NotNull(requirements, nameof(requirements));
            ArgumentGuard.NotNull(state, nameof(state));

            (IIdentifiable resource, ResourceContext _) = ConvertResourceIdentity(resourceIdentifierObject, requirements, state);
            return resource;
        }
    }
}
