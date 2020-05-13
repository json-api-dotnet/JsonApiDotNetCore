using System;
using System.Collections;
using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using Newtonsoft.Json;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Serialization.Server.Builders;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCore.Serialization.Server
{
    /// <summary>
    /// Server serializer implementation of <see cref="BaseDocumentBuilder"/>
    /// </summary>
    /// <remarks>
    /// Because in JsonApiDotNetCore every json:api request is associated with exactly one
    /// resource (the request resource, see <see cref="ICurrentRequest.GetRequestResource"/>),
    /// the serializer can leverage this information using generics.
    /// See <see cref="ResponseSerializerFactory"/> for how this is instantiated.
    /// </remarks>
    /// <typeparam name="TResource">Type of the resource associated with the scope of the request
    /// for which this serializer is used.</typeparam>
    public class ResponseSerializer<TResource> : BaseDocumentBuilder, IJsonApiSerializer, IResponseSerializer
        where TResource : class, IIdentifiable
    {
        public RelationshipAttribute RequestRelationship { get; set; }
        private readonly Dictionary<Type, List<AttrAttribute>> _attributesToSerializeCache = new Dictionary<Type, List<AttrAttribute>>();
        private readonly Dictionary<Type, List<RelationshipAttribute>> _relationshipsToSerializeCache = new Dictionary<Type, List<RelationshipAttribute>>();
        private readonly IFieldsToSerialize _fieldsToSerialize;
        private readonly IJsonApiOptions _options;
        private readonly IMetaBuilder<TResource> _metaBuilder;
        private readonly Type _primaryResourceType;
        private readonly ILinkBuilder _linkBuilder;
        private readonly IIncludedResourceObjectBuilder _includedBuilder;

        public ResponseSerializer(IMetaBuilder<TResource> metaBuilder,
            ILinkBuilder linkBuilder,
            IIncludedResourceObjectBuilder includedBuilder,
            IFieldsToSerialize fieldsToSerialize,
            IResourceObjectBuilder resourceObjectBuilder,
            IJsonApiOptions options)
            : base(resourceObjectBuilder)
        {
            _fieldsToSerialize = fieldsToSerialize;
            _options = options;
            _linkBuilder = linkBuilder;
            _metaBuilder = metaBuilder;
            _includedBuilder = includedBuilder;
            _primaryResourceType = typeof(TResource);
        }

        /// <inheritdoc/>
        public string Serialize(object data)
        {
            if (data == null || data is IIdentifiable)
            {
                return SerializeSingle((IIdentifiable)data);
            }

            if (data is IEnumerable<IIdentifiable> collectionOfIdentifiable)
            {
                return SerializeMany(collectionOfIdentifiable);
            }

            if (data is ErrorDocument errorDocument)
            {
                return SerializeErrorDocument(errorDocument);
            }

            throw new InvalidOperationException("Data being returned must be errors or resources.");
        }

        private string SerializeErrorDocument(ErrorDocument errorDocument)
        {
            return SerializeObject(errorDocument, _options.SerializerSettings, serializer => { serializer.ApplyErrorSettings(); });
        }

        /// <summary>
        /// Convert a single entity into a serialized <see cref="Document"/>
        /// </summary>
        /// <remarks>
        /// This method is set internal instead of private for easier testability.
        /// </remarks>
        internal string SerializeSingle(IIdentifiable entity)
        {
            if (RequestRelationship != null && entity != null)
            {
                var relationship = ((ResponseResourceObjectBuilder)_resourceObjectBuilder).Build(entity, RequestRelationship);
                return SerializeObject(relationship, _options.SerializerSettings, serializer => { serializer.NullValueHandling = NullValueHandling.Include; });
            }

            var (attributes, relationships) = GetFieldsToSerialize();
            var document = Build(entity, attributes, relationships);
            var resourceObject = document.SingleData;
            if (resourceObject != null)
                resourceObject.Links = _linkBuilder.GetResourceLinks(resourceObject.Type, resourceObject.Id);

            AddTopLevelObjects(document);

            return SerializeObject(document, _options.SerializerSettings, serializer => { serializer.NullValueHandling = NullValueHandling.Include; });
        }

        private (List<AttrAttribute>, List<RelationshipAttribute>) GetFieldsToSerialize()
        {
            return (GetAttributesToSerialize(_primaryResourceType), GetRelationshipsToSerialize(_primaryResourceType));
        }

        /// <summary>
        /// Convert a list of entities into a serialized <see cref="Document"/>
        /// </summary>
        /// <remarks>
        /// This method is set internal instead of private for easier testability.
        /// </remarks>
        internal string SerializeMany(IEnumerable<IIdentifiable> entities)
        {
            var (attributes, relationships) = GetFieldsToSerialize();
            var document = Build(entities, attributes, relationships);
            foreach (ResourceObject resourceObject in document.ManyData)
            {
                var links = _linkBuilder.GetResourceLinks(resourceObject.Type, resourceObject.Id);
                if (links == null)
                    break;

                resourceObject.Links = links;
            }

            AddTopLevelObjects(document);

            return SerializeObject(document, _options.SerializerSettings, serializer => { serializer.NullValueHandling = NullValueHandling.Include; });
        }

        /// <summary>
        /// Gets the list of attributes to serialize for the given <paramref name="resourceType"/>.
        /// Note that the choice of omitting null/default-values is not handled here,
        /// but in <see cref="IResourceObjectBuilderSettingsProvider"/>.
        /// </summary>
        /// <param name="resourceType">Type of entity to be serialized</param>
        /// <returns>List of allowed attributes in the serialized result</returns>
        private List<AttrAttribute> GetAttributesToSerialize(Type resourceType)
        {
            // Check the attributes cache to see if the allowed attrs for this resource type were determined before.
            if (_attributesToSerializeCache.TryGetValue(resourceType, out List<AttrAttribute> allowedAttributes))
                return allowedAttributes;

            // Get the list of attributes to be exposed for this type
            allowedAttributes = _fieldsToSerialize.GetAllowedAttributes(resourceType);

            // add to cache so we we don't have to look this up next time.
            _attributesToSerializeCache.Add(resourceType, allowedAttributes);
            return allowedAttributes;
        }

        /// <summary>
        /// By default, the server serializer exposes all defined relationships, unless
        /// in the <see cref="ResourceDefinition{T}"/> a subset to hide was defined explicitly.
        /// </summary>
        /// <param name="resourceType">Type of entity to be serialized</param>
        /// <returns>List of allowed relationships in the serialized result</returns>
        private List<RelationshipAttribute> GetRelationshipsToSerialize(Type resourceType)
        {
            // Check the relationships cache to see if the allowed attrs for this resource type were determined before.
            if (_relationshipsToSerializeCache.TryGetValue(resourceType, out List<RelationshipAttribute> allowedRelations))
                return allowedRelations;

            // Get the list of relationships to be exposed for this type
            allowedRelations = _fieldsToSerialize.GetAllowedRelationships(resourceType);
            // add to cache so we we don't have to look this up next time.
            _relationshipsToSerializeCache.Add(resourceType, allowedRelations);
            return allowedRelations;

        }

        /// <summary>
        /// Adds top-level objects that are only added to a document in the case
        /// of server-side serialization.
        /// </summary>
        private void AddTopLevelObjects(Document document)
        {
            document.Links = _linkBuilder.GetTopLevelLinks();
            document.Meta = _metaBuilder.GetMeta();
            document.Included = _includedBuilder.Build();
        }
    }
}
