using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Client.Internal
{
    /// <summary>
    /// Client deserializer implementation of the <see cref="BaseDeserializer" />.
    /// </summary>
    [PublicAPI]
    public class ResponseDeserializer : BaseDeserializer, IResponseDeserializer
    {
        public ResponseDeserializer(IResourceContextProvider resourceContextProvider, IResourceFactory resourceFactory)
            : base(resourceContextProvider, resourceFactory)
        {
        }

        /// <inheritdoc />
        public SingleResponse<TResource> DeserializeSingle<TResource>(string body)
            where TResource : class, IIdentifiable
        {
            ArgumentGuard.NotNullNorEmpty(body, nameof(body));

            object resource = DeserializeBody(body);

            return new SingleResponse<TResource>
            {
                Links = Document.Links,
                Meta = Document.Meta,
                Data = (TResource)resource,
                JsonApi = null,
                Errors = null
            };
        }

        /// <inheritdoc />
        public ManyResponse<TResource> DeserializeMany<TResource>(string body)
            where TResource : class, IIdentifiable
        {
            ArgumentGuard.NotNullNorEmpty(body, nameof(body));

            object resources = DeserializeBody(body);

            return new ManyResponse<TResource>
            {
                Links = Document.Links,
                Meta = Document.Meta,
                Data = ((ICollection<IIdentifiable>)resources)?.Cast<TResource>().ToArray(),
                JsonApi = null,
                Errors = null
            };
        }

        /// <summary>
        /// Additional processing required for client deserialization, responsible for parsing the <see cref="Document.Included" /> property. When a relationship
        /// value is parsed, it goes through the included list to set its attributes and relationships.
        /// </summary>
        /// <param name="resource">
        /// The resource that was constructed from the document's body.
        /// </param>
        /// <param name="field">
        /// The metadata for the exposed field.
        /// </param>
        /// <param name="data">
        /// Relationship data for <paramref name="resource" />. Is null when <paramref name="field" /> is not a <see cref="RelationshipAttribute" />.
        /// </param>
        protected override void AfterProcessField(IIdentifiable resource, ResourceFieldAttribute field, RelationshipEntry data = null)
        {
            ArgumentGuard.NotNull(resource, nameof(resource));
            ArgumentGuard.NotNull(field, nameof(field));

            // Client deserializers do not need additional processing for attributes.
            if (field is AttrAttribute)
            {
                return;
            }

            // if the included property is empty or absent, there is no additional data to be parsed.
            if (Document.Included.IsNullOrEmpty())
            {
                return;
            }

            if (data != null)
            {
                if (field is HasOneAttribute hasOneAttr)
                {
                    // add attributes and relationships of a parsed HasOne relationship
                    ResourceIdentifierObject rio = data.SingleData;
                    hasOneAttr.SetValue(resource, rio == null ? null : ParseIncludedRelationship(rio));
                }
                else if (field is HasManyAttribute hasManyAttr)
                {
                    // add attributes and relationships of a parsed HasMany relationship
                    IEnumerable<IIdentifiable> items = data.ManyData.Select(ParseIncludedRelationship);
                    IEnumerable values = CollectionConverter.CopyToTypedCollection(items, hasManyAttr.Property.PropertyType);
                    hasManyAttr.SetValue(resource, values);
                }
            }
        }

        /// <summary>
        /// Searches for and parses the included relationship.
        /// </summary>
        private IIdentifiable ParseIncludedRelationship(ResourceIdentifierObject relatedResourceIdentifier)
        {
            ResourceContext relatedResourceContext = ResourceContextProvider.GetResourceContext(relatedResourceIdentifier.Type);

            if (relatedResourceContext == null)
            {
                throw new InvalidOperationException($"Included type '{relatedResourceIdentifier.Type}' is not a registered JSON:API resource.");
            }

            IIdentifiable relatedInstance = ResourceFactory.CreateInstance(relatedResourceContext.ResourceType);
            relatedInstance.StringId = relatedResourceIdentifier.Id;

            ResourceObject includedResource = GetLinkedResource(relatedResourceIdentifier);

            if (includedResource != null)
            {
                SetAttributes(relatedInstance, includedResource.Attributes, relatedResourceContext.Attributes);
                SetRelationships(relatedInstance, includedResource.Relationships, relatedResourceContext.Relationships);
            }

            return relatedInstance;
        }

        private ResourceObject GetLinkedResource(ResourceIdentifierObject relatedResourceIdentifier)
        {
            try
            {
                return Document.Included.SingleOrDefault(resourceObject =>
                    resourceObject.Type == relatedResourceIdentifier.Type && resourceObject.Id == relatedResourceIdentifier.Id);
            }
            catch (InvalidOperationException exception)
            {
                throw new InvalidOperationException(
                    "A compound document MUST NOT include more than one resource object for each type and ID pair." +
                    $"The duplicate pair was '{relatedResourceIdentifier.Type}, {relatedResourceIdentifier.Id}'", exception);
            }
        }
    }
}
