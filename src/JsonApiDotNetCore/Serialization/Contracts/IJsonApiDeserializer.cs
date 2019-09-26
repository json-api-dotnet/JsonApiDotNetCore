namespace JsonApiDotNetCore.Serialization.Contracts
{
    public interface IJsonApiDeserializer
    {
        object Deserialize(string body);
    }
}
