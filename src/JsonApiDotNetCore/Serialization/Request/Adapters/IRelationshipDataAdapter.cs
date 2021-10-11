using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters
{
    /// <summary>
    /// Validates and converts the data from a relationship. It appears in a relationship request, in the relationships of a POST/PATCH resource request, in
    /// an entry of an atomic:operations request that targets a relationship and in the relationships of an operations entry that creates or updates a
    /// resource.
    /// </summary>
    public interface IRelationshipDataAdapter
    {
        /// <summary>
        /// Validates and converts the specified <paramref name="data" />.
        /// </summary>
        object? Convert(SingleOrManyData<ResourceObject> data, RelationshipAttribute relationship, bool useToManyElementType, RequestAdapterState state);

        /// <summary>
        /// Validates and converts the specified <paramref name="data" />.
        /// </summary>
        object? Convert(SingleOrManyData<ResourceIdentifierObject> data, RelationshipAttribute relationship, bool useToManyElementType,
            RequestAdapterState state);
    }
}
