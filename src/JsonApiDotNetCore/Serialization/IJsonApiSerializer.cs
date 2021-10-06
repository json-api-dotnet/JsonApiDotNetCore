namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Serializer used internally in JsonApiDotNetCore to serialize responses.
    /// </summary>
    public interface IJsonApiSerializer
    {
        /// <summary>
        /// Serializes a single resource or a collection of resources.
        /// </summary>
        string Serialize(object content);
    }
}
