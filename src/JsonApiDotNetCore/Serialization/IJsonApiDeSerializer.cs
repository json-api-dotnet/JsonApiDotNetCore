using System.Collections.Generic;

namespace JsonApiDotNetCore.Serialization
{
    public interface IJsonApiDeSerializer
    {
        object Deserialize(string requestBody);
        object DeserializeRelationship(string requestBody);
        List<TEntity> DeserializeList<TEntity>(string requestBody);
    }
}