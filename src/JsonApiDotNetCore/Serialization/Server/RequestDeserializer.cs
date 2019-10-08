using System;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Serialization.Server
{
    /// <summary>
    /// Server deserializer implementation of the <see cref="BaseDocumentParser"/>
    /// </summary>
    public class RequestDeserializer : BaseDocumentParser, IJsonApiDeserializer
    {
        private readonly ITargetedFields  _targetedFields;

        public RequestDeserializer(IResourceGraph resourceGraph,
                                  ITargetedFields  updatedFields) : base(resourceGraph)
        {
            _targetedFields = updatedFields;
        }

        /// <inheritdoc/>
        public new object Deserialize(string body)
        {
            return base.Deserialize(body);
        }

        /// <summary>
        /// Additional procesing required for server deserialization. Flags a
        /// processed attribute or relationship as updated using <see cref="ITargetedFields"/>.
        /// </summary>
        /// <param name="entity">The entity that was constructed from the document's body</param>
        /// <param name="field">The metadata for the exposed field</param>
        /// <param name="data">Relationship data for <paramref name="entity"/>. Is null when <paramref name="field"/> is not a <see cref="RelationshipAttribute"/></param>
        protected override void AfterProcessField(IIdentifiable entity, IResourceField field, RelationshipData data = null)
        {
            if (field is AttrAttribute attr)
            {
                if (!attr.IsImmutable)
                    _targetedFields.Attributes.Add(attr);
                else
                    throw new InvalidOperationException($"Attribute {attr.PublicAttributeName} is immutable and therefore cannot be updated.");
            }
            else if (field is RelationshipAttribute relationship)
                _targetedFields.Relationships.Add(relationship);
        }
    }
}
