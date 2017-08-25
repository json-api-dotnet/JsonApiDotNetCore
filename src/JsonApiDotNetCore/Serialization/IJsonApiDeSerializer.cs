using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Serialization
{
    public interface IJsonApiDeSerializer
    {
        object Deserialize(string requestBody);
        TEntity Deserialize<TEntity>(string requestBody);
        object DeserializeRelationship(string requestBody);
        List<TEntity> DeserializeList<TEntity>(string requestBody);
        object DocumentToObject(DocumentData data);
    }
}