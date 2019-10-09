using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Client
{

    /// <summary>
    /// Client serializer implementation of <see cref="BaseDocumentBuilder"/>
    /// Note that this implementation does not override the default implementation
    /// of <see cref="ResourceObjectBuilder.GetRelationshipData"/>.
    /// </summary>
    public class RequestSerializer : BaseDocumentBuilder, IRequestSerializer
    {
        private readonly Dictionary<Type, List<AttrAttribute>> _attributesToSerializeCache;
        private readonly Dictionary<Type, List<RelationshipAttribute>> _relationshipsToSerializeCache;
        private Type _currentTargetedResource;
        private readonly IFieldsExplorer _fieldExplorer;
        public RequestSerializer(IFieldsExplorer fieldExplorer,
                                IContextEntityProvider provider,
                                IResourceObjectBuilder resourceObjectBuilder)
            : base(resourceObjectBuilder, provider)
        {
            _fieldExplorer = fieldExplorer;
            _attributesToSerializeCache = new Dictionary<Type, List<AttrAttribute>>();
            _relationshipsToSerializeCache = new Dictionary<Type, List<RelationshipAttribute>>();
        }

        /// <inheritdoc/>
        public string Serialize(IIdentifiable entity)
        {
            if (entity == null)
                return JsonConvert.SerializeObject(Build(entity, new List<AttrAttribute>(), new List<RelationshipAttribute>()));

            _currentTargetedResource = entity?.GetType();
            var document = Build(entity, GetAttributesToSerialize(entity), GetRelationshipsToSerialize(entity));
            _currentTargetedResource = null;
            return JsonConvert.SerializeObject(document);
        }

        /// <inheritdoc/>
        public string Serialize(IEnumerable entities)
        {
            IIdentifiable entity = null;
            foreach (IIdentifiable item in entities)
            {
                entity = item;
                break;
            }
            if (entity == null)
                return JsonConvert.SerializeObject(Build(entities, new List<AttrAttribute>(), new List<RelationshipAttribute>()));

            _currentTargetedResource = entity?.GetType();
            var attributes = GetAttributesToSerialize(entity);
            var relationships = GetRelationshipsToSerialize(entity);
            var document = base.Build(entities, attributes, relationships);
            _currentTargetedResource = null;
            return JsonConvert.SerializeObject(document);
        }

        /// <inheritdoc/>
        public void SetAttributesToSerialize<TResource>(Expression<Func<TResource, dynamic>> filter)
            where TResource : class, IIdentifiable
        {
            var allowedAttributes = _fieldExplorer.GetAttributes(filter);
            _attributesToSerializeCache[typeof(TResource)] = allowedAttributes;
        }

        /// <inheritdoc/>
        public void SetRelationshipsToSerialize<TResource>(Expression<Func<TResource, dynamic>> filter)
            where TResource : class, IIdentifiable
        {
            var allowedRelationships = _fieldExplorer.GetRelationships(filter);
            _relationshipsToSerializeCache[typeof(TResource)] = allowedRelationships;
        }

        /// <summary>
        /// By default, the client serializer includes all attributes in the result,
        /// unless a list of allowed attributes was supplied using the <see cref="SetAttributesToSerialize"/>
        /// method. For any related resources, attributes are never exposed.
        /// </summary>
        private List<AttrAttribute> GetAttributesToSerialize(IIdentifiable entity)
        {
            var resourceType = entity.GetType();
            if (_currentTargetedResource != resourceType)
                // We're dealing with a relationship that is being serialized, for which
                // we never want to include any attributes in the payload.
                return new List<AttrAttribute>();

            if (!_attributesToSerializeCache.TryGetValue(resourceType, out var attributes))
                return _fieldExplorer.GetAttributes(resourceType);

            return attributes;
        }

        /// <summary>
        /// By default, the client serializer does not include any relationships
        /// for entities in the primary data unless explicitly included using
        /// <see cref="SetRelationshipsToSerialize{T}(Expression{Func{T, dynamic}})"/>.
        /// </summary>
        private List<RelationshipAttribute> GetRelationshipsToSerialize(IIdentifiable entity)
        {
            var currentResourceType = entity.GetType();
            /// only allow relationship attributes to be serialized if they were set using
            /// <see cref="RelationshipsToInclude{T}(Expression{Func{T, dynamic}})"/>
            /// and the current <paramref name="entity"/> is a main entry in the primary data.
            if (!_relationshipsToSerializeCache.TryGetValue(currentResourceType, out var relationships))
                return new List<RelationshipAttribute>();

            return relationships;
        }
    }
}