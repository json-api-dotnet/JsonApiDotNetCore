using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Deserializer used internally in JsonApiDotNetCore to deserialize requests.
    /// </summary>
    public interface IJsonApiDeserializer
    {
        /// <summary>
        /// Deserializes JSON into a <see cref="Document"/> and constructs resources
        /// from <see cref="ExposableData{T}.Data"/>.
        /// </summary>
        /// <param name="body">The JSON to be deserialized.</param>
        /// <returns>The resources constructed from the content.</returns>
        object Deserialize(string body);

        /// <summary>
        /// Deserializes JSON into a <see cref="OperationsDocument"/> and constructs entities
        /// from <see cref="ExposableData{T}.Data"/>.
        /// </summary>
        /// <param name="body">The JSON to be deserialized</param>
        /// <returns>The operations document constructed from the content</returns>
        object DeserializeOperationsRequestDocument(string body);

        /// <summary>
        /// Creates an instance of the referenced type in <paramref name="data"/>
        /// and sets its attributes and relationships
        /// </summary>
        IIdentifiable CreateResourceFromObject(ResourceObject data);
    }
}
