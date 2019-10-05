using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Query;
using Newtonsoft.Json;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Serialization.Server.Builders;

namespace JsonApiDotNetCore.Serialization.Server
{
    /// <summary>
    /// Server serializer implementation of <see cref="DocumentBuilder"/>
    /// </summary>
    /// <remarks>
    /// Because in JsonApiDotNetCore every json:api request is associated with exactly one
    /// resource (the request resource, see <see cref="ICurrentRequest.GetRequestResource"/>),
    /// the serializer can leverage this information using generics.
    /// See <see cref="ResponseSerializerFactory"/> for how this is instantiated.
    /// </remarks>
    /// <typeparam name="TResource">Type of the resource associated with the scope of the request
    /// for which this serializer is used.</typeparam>
    public class ResponseSerializer<TResource> : DocumentBuilder, IJsonApiSerializer
        where TResource : class, IIdentifiable
    {
        private readonly Dictionary<Type, List<AttrAttribute>> _attributesToSerializeCache = new Dictionary<Type, List<AttrAttribute>>();
        private readonly Dictionary<Type, List<RelationshipAttribute>> _relationshipsToSerializeCache = new Dictionary<Type, List<RelationshipAttribute>>();
        private readonly IIncludeService _includeQuery;
        private readonly IFieldsToSerialize _fieldsToSerialize;
        private readonly IMetaBuilder<TResource> _metaBuilder;
        private readonly Type _primaryResourceType;
        private readonly ILinkBuilder _linkBuilder;
        private readonly IIncludedResourceObjectBuilder _includedBuilder;
        private bool _requestRelationshipProvided;

        public ResponseSerializer(IMetaBuilder<TResource> metaBuilder,
                                  ILinkBuilder linkBuilder,
                                  IIncludedResourceObjectBuilder includedBuilder,
                                  IFieldsToSerialize fieldsToSerialize,
                                  IIncludeService includeQuery,
                                  IResourceGraph resourceGraph,
                                  IContextEntityProvider provider,
                                  ISerializerSettingsProvider settingsProvider)
            : base(resourceGraph, provider, settingsProvider.Get())
        {
            _includeQuery = includeQuery;
            _fieldsToSerialize = fieldsToSerialize;
            _linkBuilder = linkBuilder;
            _metaBuilder = metaBuilder;
            _includedBuilder = includedBuilder;
            _primaryResourceType = typeof(TResource);
        }

        /// <inheritdoc/>
        public string Serialize(object data, RelationshipAttribute requestRelationship = null)
        {
            if (data is IEnumerable entities)
                return SerializeMany(entities);
            return SerializeSingle((IIdentifiable)data, requestRelationship);
        }

        /// <summary>
        /// Convert a single entity into a serialized <see cref="Document"/>
        /// </summary>
        /// <remarks>
        /// This method is set internal instead of private for easier testability.
        /// </remarks>
        internal string SerializeSingle(IIdentifiable entity, RelationshipAttribute requestRelationship = null)
        {
            if (requestRelationship != null)
            {
                _requestRelationshipProvided = true;
                return JsonConvert.SerializeObject(GetRelationshipData(requestRelationship, entity));
            }

            var (attributes, relationships) = GetFieldsToSerialize();
            var document = Build(entity, attributes, relationships);
            var resourceObject = document.SingleData;
            if (resourceObject != null)
                resourceObject.Links = _linkBuilder.GetResourceLinks(resourceObject.Type, resourceObject.Id);

            AddTopLevelObjects(document);
            return JsonConvert.SerializeObject(document);

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
        internal string SerializeMany(IEnumerable entities)
        {
            var (attributes, relationships) = GetFieldsToSerialize();
            var document = Build(entities, attributes, relationships);
            foreach (ResourceObject resourceObject in (IEnumerable)document.Data)
            {
                var links = _linkBuilder.GetResourceLinks(resourceObject.Type, resourceObject.Id);
                if (links == null)
                    break;

                resourceObject.Links = links;
            }

            AddTopLevelObjects(document);
            return JsonConvert.SerializeObject(document);
        }


        /// <summary>
        /// Gets the list of attributes to serialize for the given <paramref name="resourceType"/>.
        /// Note that the choice omitting null-values is not handled here,
        /// but in <see cref="ISerializerSettingsProvider"/>.
        /// </summary>
        /// <param name="resourceType">Type of entity to be serialized</param>
        /// <returns>List of allowed attributes in the serialized result</returns>
        private List<AttrAttribute> GetAttributesToSerialize(Type resourceType)
        {
            /// Check the attributes cache to see if the allowed attrs for this resource type were determined before.
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
            /// Check the relationships cache to see if the allowed attrs for this resource type were determined before.
            if (_relationshipsToSerializeCache.TryGetValue(resourceType, out List<RelationshipAttribute> allowedRelations))
                return allowedRelations;

            // Get the list of relationships to be exposed for this type
            allowedRelations = _fieldsToSerialize.GetAllowedRelationships(resourceType);
            // add to cache so we we don't have to look this up next time.
            _relationshipsToSerializeCache.Add(resourceType, allowedRelations);
            return allowedRelations;

        }

        /// <summary>
        /// Builds the values of the relationships object on a resource object.
        /// The server serializer only populates the "data" member when the relationship is included,
        /// and adds links unless these are turned off. This means that if a relationship is not included
        /// and links are turned off, the entry would be completely empty, ie { }, which is not conform
        /// json:api spec. In that case we return null which will omit the entry from the output.
        /// </summary>
        protected override RelationshipData GetRelationshipData(RelationshipAttribute relationship, IIdentifiable entity)
        {
            RelationshipData relationshipData = null;

            if (_requestRelationshipProvided)
            {   // if serializing a request with a requestRelationship, always populate data field.
                relationshipData = base.GetRelationshipData(relationship, entity);
            }
            else if (ShouldInclude(relationship, out var relationshipChain))
            {   // if the relationship is included, populate the "data" field.
                relationshipData = base.GetRelationshipData(relationship, entity);
                if (relationshipData.HasData)
                    _includedBuilder.IncludeRelationshipChain(relationshipChain, entity);
            }

            var links = _linkBuilder.GetRelationshipLinks(relationship, entity);
            if (links != null)
            {   // if links relationshiplinks should be built for this entry, populate the "links" field.
                relationshipData = relationshipData ?? new RelationshipData();
                relationshipData.Links = links;
            }

            /// if neither "links" nor "data" was popupated, return null, which will omit this entry from the output.
            /// (see the NullValueHandling settings on <see cref="ResourceObject"/>)
            return relationshipData;
        }

        /// <summary>
        /// Adds top-level objects that are only added to a document in the case
        /// of server-side serialization.
        /// </summary>
        private void AddTopLevelObjects(Document document)
        {
            document.Links = _linkBuilder.GetTopLevelLinks(_provider.GetContextEntity<TResource>());
            document.Meta = _metaBuilder.GetMeta();
            document.Included = _includedBuilder.Build();
        }

        /// <summary>
        /// Inspects the included relationship chains (see <see cref="IIncludeService"/>
        /// to see if <paramref name="relationship"/> should be included or not.
        /// </summary>
        private bool ShouldInclude(RelationshipAttribute relationship, out List<RelationshipAttribute> inclusionChain)
        {
            inclusionChain = _includeQuery.Get()?.SingleOrDefault(l => l.First().Equals(relationship));
            if (inclusionChain == null)
                return false;
            return true;
        }
    }
}
