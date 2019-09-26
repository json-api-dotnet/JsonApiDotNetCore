using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Serialization.Contracts
{
    public interface IClientDeserializer
    {
        DeserializedSingleResponse<TResource> DeserializeSingle<TResource>(string body) where TResource : class, IIdentifiable;
        DeserializedListResponse<TResource> DeserializeList<TResource>(string body) where TResource : class, IIdentifiable;
    }
}