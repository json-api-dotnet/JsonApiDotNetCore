using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters;

/// <summary>
/// Validates and converts the data from a resource in a POST/PATCH resource request.
/// </summary>
public interface IResourceDataAdapter
{
    /// <summary>
    /// Validates and converts the specified <paramref name="data" />.
    /// </summary>
    IIdentifiable Convert(SingleOrManyData<ResourceObject> data, ResourceIdentityRequirements requirements, RequestAdapterState state);
}
