namespace JsonApiDotNetCore.Serialization
{
    public interface IJsonApiSerializerFactory
    {
        /// <summary>
        /// Instantiates the serializer to process the servers response.
        /// </summary>
        IJsonApiSerializer GetSerializer();
    }
}