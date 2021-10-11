#nullable disable

using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters
{
    /// <summary>
    /// Validates and converts the data from an entry in an atomic:operations request that creates or updates a resource.
    /// </summary>
    public interface IResourceDataInOperationsRequestAdapter
    {
        /// <summary>
        /// Validates and converts the specified <paramref name="data" />.
        /// </summary>
        IIdentifiable Convert(SingleOrManyData<ResourceObject> data, ResourceIdentityRequirements requirements, RequestAdapterState state);
    }
}
