using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters;

/// <summary>
/// Validates and converts a <see cref="ResourceObject" />. It appears in a POST/PATCH resource request and an entry in an atomic:operations request that
/// creates or updates a resource.
/// </summary>
public interface IResourceObjectAdapter
{
    /// <summary>
    /// Validates and converts the specified <paramref name="resourceObject" />.
    /// </summary>
    (IIdentifiable resource, ResourceType resourceType) Convert(ResourceObject resourceObject, ResourceIdentityRequirements requirements,
        RequestAdapterState state);
}
