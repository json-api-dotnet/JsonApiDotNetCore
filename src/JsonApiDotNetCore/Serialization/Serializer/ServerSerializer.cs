using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Builders
{

    public class ServerSerializerFactory : IJsonApiSerializerFactory
    {
        private readonly IRequestManager _requestManager;
        private readonly IServiceProvider _provider;

        public ServerSerializerFactory(IRequestManager requestManager, IServiceProvider provider)
        {
            _requestManager = requestManager;
            _provider = provider;
        }
        public IJsonApiSerializer GetSerializer()
        {
            var serializerType = typeof(ServerSerializer<>).MakeGenericType(_requestManager.GetRequestResource().EntityType);
            return (IJsonApiSerializer)_provider.GetRequiredService(serializerType);
        }
    }

    public class ServerSerializer<T> : DocumentBuilder, IJsonApiSerializer where T : class, IIdentifiable
    {
        private readonly Dictionary<Type, List<AttrAttribute>> _attributesToSerializeCache = new Dictionary<Type, List<AttrAttribute>>();
        private readonly Dictionary<Type, List<RelationshipAttribute>> _relationshipsToSerializeCache = new Dictionary<Type, List<RelationshipAttribute>>();
        private readonly IIncludedQueryService _includedQuery;
        private readonly IFieldsQueryService _fieldQuery;
        private readonly ISerializableFields _serializableFields;
        private readonly IMetaBuilder<T> _metaBuilder;
        private readonly Type _requestResourceType;
        private readonly ILinkBuilder _linkBuilder;
        private readonly IIncludedRelationshipsBuilder _includedBuilder;

        public ServerSerializer(
            IMetaBuilder<T> metaBuilder,
            ILinkBuilder linkBuilder,
            IIncludedRelationshipsBuilder includedBuilder,
            ISerializableFields serializableFields,
            IIncludedQueryService includedQuery,
            IFieldsQueryService fieldQuery,
            IResourceGraph resourceGraph, IContextEntityProvider provider) : base(resourceGraph, provider)
        {
            _includedQuery = includedQuery;
            _fieldQuery = fieldQuery;
            _serializableFields = serializableFields;
            _linkBuilder = linkBuilder;
            _metaBuilder = metaBuilder;
            _includedBuilder = includedBuilder;
            _requestResourceType = typeof(T);
        }

        public string Serialize(object content)
        {
            if (content is IEnumerable entities)
                return SerializeMany(entities);
            return SerializeSingle((IIdentifiable)content);
        }

        internal string SerializeSingle(IIdentifiable entity)
        {
            var attributes = GetAttributesToSerialize(_requestResourceType);
            var relationships = GetRelationshipsToSerialize(_requestResourceType);
            var document = Build(entity, attributes, relationships);
            var resourceObject = document.SingleData;
            if (resourceObject != null) resourceObject.Links = _linkBuilder.GetResourceLinks(resourceObject.Type, resourceObject.Id);
            AddTopLevelObjects(document);
            return GetStringOutput(document);
        }

        internal string SerializeMany(IEnumerable entities)
        {
            var attributes = GetAttributesToSerialize(_requestResourceType);
            var relationships = GetRelationshipsToSerialize(_requestResourceType);
            var document = Build(entities, attributes, relationships);
            foreach (ResourceObject resourceObject in (IEnumerable)document.Data)
            {
                var links = _linkBuilder.GetResourceLinks(resourceObject.Type, resourceObject.Id);
                if (links == null)
                    break;

                resourceObject.Links = links;
            }
            AddTopLevelObjects(document);
            return GetStringOutput(document);
        }

        /// <summary>
        /// Gets the list of attributes to serialize for the given <paramref name="resourceType"/>.
        /// Depending on if instance-dependent attribute hiding was implemented in the corresponding
        /// <see cref="ResourceDefinition{T}"/>, the server serializer caches the output list of attributes
        /// or recalculates it for every instance. Note that the choice omitting null-values
        /// is not handled here, but in <see cref="DocumentBuilderOptionsProvider"/>.
        /// </summary>
        /// <param name="resourceType">Type of entity to be serialized</param>
        /// <returns>List of allowed attributes in the serialized result</returns>
        private List<AttrAttribute> GetAttributesToSerialize(Type resourceType)
        {
            /// Check the attributes cache to see if the allowed attrs for this resource type were determined before.
            if (_attributesToSerializeCache.TryGetValue(resourceType, out List<AttrAttribute> allowedAttributes))
                return allowedAttributes;

            // Get the list of attributes to be exposed for this type
            allowedAttributes = _serializableFields.GetAllowedAttributes(resourceType);
            var fields = _fieldQuery.Get();
            if (fields != null)
                // from the allowed attributes, select the ones flagged by sparse field selection.
                allowedAttributes = allowedAttributes.Where(attr => !fields.Contains(attr)).ToList();

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
            allowedRelations = _serializableFields.GetAllowedRelationships(resourceType);
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
        /// <param name="relationship"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected override RelationshipData GetRelationshipData(RelationshipAttribute relationship, IIdentifiable entity)
        {
            RelationshipData relationshipData = null;
            /// if the relationship is included, populate the "data" field.
            if (ShouldInclude(relationship, out var relationshipChain))
            {
                relationshipData = base.GetRelationshipData(relationship, entity);
                if (relationshipData.HasData)
                    _includedBuilder.IncludeRelationshipChain(relationshipChain, entity);
            }

            var links = _linkBuilder.GetRelationshipLinks(relationship, entity);
            /// if links relationshiplinks should be built for this entry, populate the "links" field.
            if (links != null)
            {
                relationshipData = relationshipData ?? new RelationshipData();
                relationshipData.Links = links;
            }

            /// if neither "links" nor "data" was popupated, return null, which will omit this entry from the output.
            return relationshipData;
        }

        private void AddTopLevelObjects(Document document)
        {
            document.Links = _linkBuilder.GetTopLevelLinks();
            document.Meta = _metaBuilder.GetMeta();
            document.Included = _includedBuilder.Build();
        }

        private bool ShouldInclude(RelationshipAttribute relationship, out List<RelationshipAttribute> inclusionChain)
        {
            inclusionChain = _includedQuery.Get()?.SingleOrDefault(l => l.First().Equals(relationship));
            if (inclusionChain == null)
                return false;
            return true;
        }
    }
}
