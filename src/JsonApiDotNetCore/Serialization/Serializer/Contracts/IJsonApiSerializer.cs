
namespace JsonApiDotNetCore.Serialization.Serializer.Contracts
{
    /// <summary>
    /// Serializer used internally in JsonApiDotNetCore to serialize requests.
    /// </summary>
    public interface IJsonApiSerializer
    {
        /// <summary>
        /// Serialize a single entity or a list of entities.
        /// </summary>
        string Serialize(object content);
    }
}