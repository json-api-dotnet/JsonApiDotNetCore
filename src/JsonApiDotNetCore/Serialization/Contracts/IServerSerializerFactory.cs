namespace JsonApiDotNetCore.Builders
{
    public interface IJsonApiSerializerFactory
    {
        IJsonApiSerializer GetSerializer();
    }
}