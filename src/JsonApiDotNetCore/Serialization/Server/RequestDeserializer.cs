using System;
using System.Linq;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Internal;
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

        public RequestDeserializer(IResourceContextProvider contextProvider, IResourceFactory resourceFactory, ITargetedFields  targetedFields) 
            : base(contextProvider, resourceFactory)
        {
            _targetedFields = targetedFields;
        }

        /// <inheritdoc/>
        public new object Deserialize(string body)
        {
            return base.Deserialize(body);
        }

        /// <summary>
        /// Additional processing required for server deserialization. Flags a
        /// processed attribute or relationship as updated using <see cref="ITargetedFields"/>.
        /// </summary>
        /// <param name="entity">The entity that was constructed from the document's body</param>
        /// <param name="field">The metadata for the exposed field</param>
        /// <param name="data">Relationship data for <paramref name="entity"/>. Is null when <paramref name="field"/> is not a <see cref="RelationshipAttribute"/></param>
        protected override void AfterProcessField(IIdentifiable entity, IResourceField field, RelationshipEntry data = null)
        {
            if (field is AttrAttribute attr)
            {
                if (!attr.Capabilities.HasFlag(AttrCapabilities.AllowMutate))
                    throw new InvalidRequestBodyException(
                           "Changing the value of the requested attribute is not allowed.",
                           $"Changing the value of '{attr.PublicAttributeName}' is not allowed.", null);

                var property = attr.PropertyInfo;
                var requiredOnPost = property.GetCustomAttributes(typeof(RequiredOnPostAttribute), false);
                if (requiredOnPost?.FirstOrDefault() != null && attr.GetValue(entity) == null)
                    throw new InvalidRequestBodyException("Changing the value of a required attribute to null is not allowed.", 
                        $"Attribute '{attr.PublicAttributeName}' is required and therefore cannot be updated to null.", null);

                _targetedFields.Attributes.Add(attr);

            }
            else if (field is RelationshipAttribute relationship)
                _targetedFields.Relationships.Add(relationship);
        }
    }
}
