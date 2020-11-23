using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Serialization.Deserializer
{
    public interface IOperationsDeserializer
    {
        object Deserialize(string body);
        object DocumentToObject(ResourceObject data, List<ResourceObject> included = null);
    }
}
