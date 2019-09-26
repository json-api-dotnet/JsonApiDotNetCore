using System;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization.Contracts;

namespace JsonApiDotNetCore.Serialization
{
    public class ServerDeserializer : DocumentParser, IJsonApiDeserializer
    {
        protected readonly IUpdatedFields  _updatedFields;

        public ServerDeserializer(IResourceGraph resourceGraph,
                                  IUpdatedFields  updatedFields) : base(resourceGraph)
        {
            _updatedFields = updatedFields;
        }

        public new object Deserialize(string body)
        {
            return base.Deserialize(body);
        }

        protected override void AfterProcessField(IIdentifiable entity, IResourceField field, RelationshipData data = null)
        {
            if (field is AttrAttribute attr)
            {
                if (!attr.IsImmutable)
                    _updatedFields.AttributesToUpdate.Add(attr);
                else
                    throw new InvalidOperationException($"Attribute {attr.PublicAttributeName} is immutable and therefore cannot be updated.");
            }
            else if (field is RelationshipAttribute relationship)
                _updatedFields.RelationshipsToUpdate.Add(relationship);
        }
    }
}
