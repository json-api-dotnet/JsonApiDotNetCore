using System.Collections;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Serialization.Serializer.Contracts
{
    public interface IJsonApiSerializer
    {
        string Serialize(object content);
    }
}