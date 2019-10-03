namespace JsonApiDotNetCore.Serialization.Serializer
{
    /// <summary>
    /// Service that provides the server serializer with <see cref="SerializerSettings"/> 
    /// </summary>
    public interface ISerializerSettingsProvider
    {
        /// <summary>
        /// Gets the behaviour for the serializer it is injected in.
        /// </summary>
        SerializerSettings Get();
    }
}
