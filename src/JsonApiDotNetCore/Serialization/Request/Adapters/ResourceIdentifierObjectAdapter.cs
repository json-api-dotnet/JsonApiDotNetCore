using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters;

/// <inheritdoc cref="IResourceIdentifierObjectAdapter" />
public sealed class ResourceIdentifierObjectAdapter(IResourceGraph resourceGraph, IResourceFactory resourceFactory)
    : ResourceIdentityAdapter(resourceGraph, resourceFactory), IResourceIdentifierObjectAdapter
{
    /// <inheritdoc />
    public IIdentifiable Convert(ResourceIdentifierObject resourceIdentifierObject, ResourceIdentityRequirements requirements, RequestAdapterState state)
    {
        ArgumentGuard.NotNull(resourceIdentifierObject);
        ArgumentGuard.NotNull(requirements);
        ArgumentGuard.NotNull(state);

        (IIdentifiable resource, _) = ConvertResourceIdentity(resourceIdentifierObject, requirements, state);
        return resource;
    }
}
