namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Serializer used internally in JsonApiDotNetCore to serialize responses.
    /// </summary>
    public interface IJsonApiSerializer
    {
        /// <summary>
        /// Gets the Content-Type HTTP header value.
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Serializes a single resource or a collection of resources.
        /// </summary>
        string Serialize(object content);
    }
}
