namespace JsonApiDotNetCore.Serialization.Serializer
{
    public interface ISerializerBehaviourProvider
    {
        SerializerBehaviour GetBehaviour();
    }
}
