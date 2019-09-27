namespace JsonApiDotNetCore.Serialization.Serializer.Contracts
{
    public interface IJsonApiSerializerFactory
    {
        IJsonApiSerializer GetSerializer();
    }
}