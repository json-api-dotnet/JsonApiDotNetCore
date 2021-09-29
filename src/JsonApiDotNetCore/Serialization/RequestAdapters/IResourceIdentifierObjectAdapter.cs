using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.RequestAdapters
{
    /// <summary>
    /// Validates and converts a <see cref="ResourceIdentifierObject" />. It appears in the data object(s) of a relationship.
    /// </summary>
    public interface IResourceIdentifierObjectAdapter
    {
        /// <summary>
        /// Validates and converts the specified <paramref name="resourceIdentifierObject" />.
        /// </summary>
        IIdentifiable Convert(ResourceIdentifierObject resourceIdentifierObject, ResourceIdentityRequirements requirements, RequestAdapterState state);
    }
}
