namespace JsonApiDotNetCore.Serialization.Serializer.Contracts
{
    public interface IJsonApiSerializerFactory
    {
        /// <summary>
        /// Instantiates the serializer to process the servers response.
        /// </summary>
        IJsonApiSerializer GetSerializer();
    }
}