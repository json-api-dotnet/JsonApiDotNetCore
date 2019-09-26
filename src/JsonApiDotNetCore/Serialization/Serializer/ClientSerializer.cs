using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Builders
{
    public class ClientSerializer : DocumentBuilder
    {
        private readonly Dictionary<Type, List<AttrAttribute>> _attributesToSerializeCache = new Dictionary<Type, List<AttrAttribute>>();
        private readonly Dictionary<Type, List<RelationshipAttribute>> _relationshipsToSerializeCache = new Dictionary<Type, List<RelationshipAttribute>>();
        private Type _currentTargetedResource;
        private readonly IExposedFieldExplorer _fieldExplorer;
        public ClientSerializer(IExposedFieldExplorer fieldExplorer,
                                IContextEntityProvider provider,
                                IResourceGraph resourceGraph) : base(resourceGraph, provider)
        {
            _fieldExplorer = fieldExplorer;
        }

        /// <summary>
        /// Creates and serializes a document for a single intance of a resource.
        /// </summary>
        /// <param name="entity">Entity to serialize</param>
        /// <returns>The serialized content</returns>
        public string Serialize(IIdentifiable entity)
        {
            if (entity == null)
                return GetStringOutput(base.Build(entity));

            _currentTargetedResource = entity?.GetType();
            var attributes = GetAttributesToSerialize(entity);
            var relationships = GetRelationshipsToSerialize(entity);
            var document = base.Build(entity, attributes, relationships);
            _currentTargetedResource = null;
            return GetStringOutput(document);

        }

        /// <summary>
        /// Creates and serializes a document for for a list of entities of one resource.
        /// </summary>
        /// <param name="entities">Entities to serialize</param>
        /// <returns>The serialized content</returns>
        public string Serialize(IEnumerable entities)
        {
            IIdentifiable entity = null;
            foreach (IIdentifiable item in entities)
            {
                entity = item;
                break;
            }
            if (entity == null)
                return GetStringOutput(base.Build(entities));

            _currentTargetedResource = entity?.GetType();
            var attributes = GetAttributesToSerialize(entity);
            var relationships = GetRelationshipsToSerialize(entity);
            var document = base.Build(entities, attributes, relationships);
            _currentTargetedResource = null;
            return GetStringOutput(document);
        }

        /// <summary>
        /// Sets the <see cref="AttrAttribute"/>s to serialize for resources of type <typeparamref name="T"/>.
        /// If no <see cref="AttrAttribute"/>s are specified, by default all attributes are included in the serialization result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        public void SetAttributesToSerialize<T>(Expression<Func<T, dynamic>> filter) where T : class, IIdentifiable
        {
            var allowedAttributes = _fieldExplorer.GetAttributes(filter);
            _attributesToSerializeCache[typeof(T)] = allowedAttributes;
        }

        /// <summary>
        /// Sets the <see cref="RelationshipAttribute"/>s to serialize for resources of type <typeparamref name="T"/>.
        /// If no <see cref="RelationshipAttribute"/>s are specified, by default no relationships are included in the serialization result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        public void SetRelationshipsToSerialize<T>(Expression<Func<T, dynamic>> filter) where T : class, IIdentifiable
        {
            var allowedRelationships = _fieldExplorer.GetRelationships(filter);
            _relationshipsToSerializeCache[typeof(T)] = allowedRelationships;
        }

        /// <summary>
        /// By default, the client serializer includes all attributes in the result,
        /// unless a list of allowed attributes was supplied using the <see cref="SetAttributesToSerialize"/>
        /// method. For any related resources, attributes are never exposed.
        /// </summary>
        /// <param name="entity">Entity to be serialized</param>
        /// <returns>List of allowed attributes in the serialized result.</returns>
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
        /// <param name="entity">Entity to be serialized</param>
        /// <returns>List of allowed relationships in the serialized result.</returns>
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