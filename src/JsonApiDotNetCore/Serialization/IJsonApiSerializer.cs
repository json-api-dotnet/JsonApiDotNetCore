namespace JsonApiDotNetCore.Serialization
{
    public interface IJsonApiSerializer
    {
        string Serialize(object entity);
    }
}