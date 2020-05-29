using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCore.Serialization.Client
{
    /// <summary>
    /// Client deserializer implementation of the <see cref="BaseDocumentParser"/>
    /// </summary>
    public class ResponseDeserializer : BaseDocumentParser, IResponseDeserializer
    {
        public ResponseDeserializer(IResourceContextProvider contextProvider, IResourceFactory resourceFactory) : base(contextProvider, resourceFactory) { }

        /// <inheritdoc/>
        public DeserializedSingleResponse<TResource> DeserializeSingle<TResource>(string body) where TResource : class, IIdentifiable
        {
            var resource = Deserialize(body);
            return new DeserializedSingleResponse<TResource>
            {
                Links = _document.Links,
                Meta = _document.Meta,
                Data = (TResource) resource,
                JsonApi = null,
                Errors = null
            };
        }

        /// <inheritdoc/>
        public DeserializedListResponse<TResource> DeserializeList<TResource>(string body) where TResource : class, IIdentifiable
        {
            var resources = Deserialize(body);
            return new DeserializedListResponse<TResource>
            {
                Links = _document.Links,
                Meta = _document.Meta,
                Data = ((ICollection<IIdentifiable>) resources)?.Cast<TResource>().ToList(),
                JsonApi = null,
                Errors = null
            };
        }

        /// <summary>
        /// Additional processing required for client deserialization, responsible
        /// for parsing the <see cref="Document.Included"/> property. When a relationship value is parsed,
        /// it goes through the included list to set its attributes and relationships.
        /// </summary>
        /// <param name="resource">The resource that was constructed from the document's body</param>
        /// <param name="field">The metadata for the exposed field</param>
        /// <param name="data">Relationship data for <paramref name="resource"/>. Is null when <paramref name="field"/> is not a <see cref="RelationshipAttribute"/></param>
        protected override void AfterProcessField(IIdentifiable resource, ResourceFieldAttribute field, RelationshipEntry data = null)
        {
            // Client deserializers do not need additional processing for attributes.
            if (field is AttrAttribute)
                return;

            // if the included property is empty or absent, there is no additional data to be parsed.
            if (_document.Included == null || _document.Included.Count == 0)
                return;

            if (field is HasOneAttribute hasOneAttr)
            {
                // add attributes and relationships of a parsed HasOne relationship
                var rio = data.SingleData;
                hasOneAttr.SetValue(resource, rio == null ? null : ParseIncludedRelationship(hasOneAttr, rio), _resourceFactory);
            }
            else if (field is HasManyAttribute hasManyAttr)
            {  // add attributes and relationships of a parsed HasMany relationship
                var items = data.ManyData.Select(rio => ParseIncludedRelationship(hasManyAttr, rio));
                var values = items.CopyToTypedCollection(hasManyAttr.Property.PropertyType);
                hasManyAttr.SetValue(resource, values, _resourceFactory);
            }
        }

        /// <summary>
        /// Searches for and parses the included relationship
        /// </summary>
        private IIdentifiable ParseIncludedRelationship(RelationshipAttribute relationshipAttr, ResourceIdentifierObject relatedResourceIdentifier)
        {
            var relatedInstance = (IIdentifiable)_resourceFactory.CreateInstance(relationshipAttr.RightType);
            relatedInstance.StringId = relatedResourceIdentifier.Id;

            var includedResource = GetLinkedResource(relatedResourceIdentifier);
            if (includedResource == null)
                return relatedInstance;

            var resourceContext = _contextProvider.GetResourceContext(relatedResourceIdentifier.Type);
            if (resourceContext == null)
                throw new InvalidOperationException($"Included type '{relationshipAttr.RightType}' is not a registered json:api resource.");

            SetAttributes(relatedInstance, includedResource.Attributes, resourceContext.Attributes);
            SetRelationships(relatedInstance, includedResource.Relationships, resourceContext.Relationships);
            return relatedInstance;
        }

        private ResourceObject GetLinkedResource(ResourceIdentifierObject relatedResourceIdentifier)
        {
            try
            {
                return _document.Included.SingleOrDefault(r => r.Type == relatedResourceIdentifier.Type && r.Id == relatedResourceIdentifier.Id);
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidOperationException("A compound document MUST NOT include more than one resource object for each type and id pair."
                        + $"The duplicate pair was '{relatedResourceIdentifier.Type}, {relatedResourceIdentifier.Id}'", e);
            }
        }
    }
}
