using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Deserializer used internally in JsonApiDotNetCore to deserialize requests.
    /// </summary>
    public interface IJsonApiDeserializer
    {
        /// <summary>
        /// Deserializes JSON in to a <see cref="Document"/> and constructs resources
        /// from <see cref="ExposableData{T}.Data"/>.
        /// </summary>
        /// <param name="body">The JSON to be deserialized</param>
        /// <returns>The resources constructed from the content</returns>
        object Deserialize(string body);
    }
}
