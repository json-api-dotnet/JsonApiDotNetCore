using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Deserializer used internally in JsonApiDotNetCore to deserialize requests.
    /// </summary>
    public interface IJsonApiDeserializer
    {
        /// <summary>
        /// Deserializes JSON into a <see cref="Document"/> or <see cref="AtomicOperationsDocument"/> and constructs resources
        /// from <see cref="ExposableData{T}.Data"/>.
        /// </summary>
        /// <param name="body">The JSON to be deserialized.</param>
        object Deserialize(string body);
    }
}
