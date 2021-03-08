using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Serialization.Client.Internal
{
    /// <summary>
    /// Client deserializer. Currently not used internally in JsonApiDotNetCore, except for in the tests. Exposed publicly to make testing easier or to
    /// implement server-to-server communication.
    /// </summary>
    [PublicAPI]
    public interface IResponseDeserializer
    {
        /// <summary>
        /// Deserializes a response with a single resource (or null) as data.
        /// </summary>
        /// <typeparam name="TResource">
        /// The type of the resources in the primary data.
        /// </typeparam>
        /// <param name="body">
        /// The JSON to be deserialized.
        /// </param>
        SingleResponse<TResource> DeserializeSingle<TResource>(string body)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Deserializes a response with an (empty) collection of resources as data.
        /// </summary>
        /// <typeparam name="TResource">
        /// The type of the resources in the primary data.
        /// </typeparam>
        /// <param name="body">
        /// The JSON to be deserialized.
        /// </param>
        ManyResponse<TResource> DeserializeMany<TResource>(string body)
            where TResource : class, IIdentifiable;
    }
}
