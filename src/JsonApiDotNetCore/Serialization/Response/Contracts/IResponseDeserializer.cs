using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Serialization.Response.Contracts
{
    /// <summary>
    /// Client deserializer. Currently not used internally in JsonApiDotNetCore,
    /// except for in the tests. Exposed pubically to make testing easier or to implement
    /// server-to-server communication.
    /// </summary>
    public interface IResponseDeserializer
    {
        /// <summary>
        /// Deserializes a response with a single resource (or null) as data.
        /// </summary>
        /// <typeparam name="TResource">The type of the resources in the primary data</typeparam>
        /// <param name="body">The JSON to be deserialized</param>
        DeserializedSingleResponse<TResource> DeserializeSingle<TResource>(string body) where TResource : class, IIdentifiable;

        /// <summary>
        /// Deserializes a response with a (empty) list of resources as data.
        /// </summary>
        /// <typeparam name="TResource">The type of the resources in the primary data</typeparam>
        /// <param name="body">The JSON to be deserialized</param>
        DeserializedListResponse<TResource> DeserializeList<TResource>(string body) where TResource : class, IIdentifiable;
    }
}