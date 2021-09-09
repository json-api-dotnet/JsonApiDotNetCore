using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Deserializer used internally in JsonApiDotNetCore to deserialize requests.
    /// </summary>
    public interface IJsonApiDeserializer
    {
        /// <summary>
        /// Deserializes JSON into a <see cref="Document" /> and constructs resources from <see cref="ExposableData{TResource}.Data" />.
        /// </summary>
        /// <param name="body">
        /// The JSON to be deserialized.
        /// </param>
        object Deserialize(string body);
    }
}
