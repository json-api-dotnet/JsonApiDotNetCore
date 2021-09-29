using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.RequestAdapters
{
    /// <summary>
    /// Validates and converts the data from an entry in an atomic:operations request that creates or updates a resource.
    /// </summary>
    public interface IOperationResourceDataAdapter
    {
        /// <summary>
        /// Validates and converts the specified <paramref name="data" />.
        /// </summary>
        IIdentifiable Convert(SingleOrManyData<ResourceObject> data, ResourceIdentityRequirements requirements, RequestAdapterState state);
    }
}
