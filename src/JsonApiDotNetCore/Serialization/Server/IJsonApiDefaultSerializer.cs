using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Serialization.Server
{
    public interface IJsonApiDefaultSerializer : IJsonApiSerializer
    {
        void SetRequestRelationship(RelationshipAttribute requestRelationship);
    }
}
