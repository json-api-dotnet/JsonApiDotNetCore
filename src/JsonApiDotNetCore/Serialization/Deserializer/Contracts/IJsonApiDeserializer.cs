namespace JsonApiDotNetCore.Serialization.Deserializer.Contracts
{
    public interface IJsonApiDeserializer
    {
        object Deserialize(string body);
    }
}
