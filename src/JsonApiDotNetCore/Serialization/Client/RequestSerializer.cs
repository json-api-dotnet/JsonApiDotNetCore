using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Client
{
    /// <summary>
    /// Client serializer implementation of <see cref="BaseDocumentBuilder"/>
    /// </summary>
    public class RequestSerializer : BaseDocumentBuilder, IRequestSerializer
    {
        private Type _currentTargetedResource;
        private readonly IResourceGraph _resourceGraph;
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings();

        public RequestSerializer(IResourceGraph resourceGraph,
                                IResourceObjectBuilder resourceObjectBuilder)
            : base(resourceObjectBuilder)
        {
            _resourceGraph = resourceGraph;
        }

        /// <inheritdoc/>
        public string Serialize(IIdentifiable entity)
        {
            if (entity == null)
            {
                var empty = Build((IIdentifiable) null, Array.Empty<AttrAttribute>(), Array.Empty<RelationshipAttribute>());
                return SerializeObject(empty, _jsonSerializerSettings);
            }

            _currentTargetedResource = entity.GetType();
            var document = Build(entity, GetAttributesToSerialize(entity), GetRelationshipsToSerialize(entity));
            _currentTargetedResource = null;

            return SerializeObject(document, _jsonSerializerSettings);
        }

        /// <inheritdoc/>
        public string Serialize(IEnumerable<IIdentifiable> entities)
        {
            IIdentifiable entity = null;
            foreach (IIdentifiable item in entities)
            {
                entity = item;
                break;
            }

            if (entity == null)
            {
                var result = Build(entities, Array.Empty<AttrAttribute>(), Array.Empty<RelationshipAttribute>());
                return SerializeObject(result, _jsonSerializerSettings);
            }

            _currentTargetedResource = entity.GetType();
            var attributes = GetAttributesToSerialize(entity);
            var relationships = GetRelationshipsToSerialize(entity);
            var document = Build(entities, attributes, relationships);
            _currentTargetedResource = null;
            return SerializeObject(document, _jsonSerializerSettings);
        }

        /// <inheritdoc/>
        public IEnumerable<AttrAttribute> AttributesToSerialize { private get; set; }

        /// <inheritdoc/>
        public IEnumerable<RelationshipAttribute> RelationshipsToSerialize { private get; set; }

        /// <summary>
        /// By default, the client serializer includes all attributes in the result,
        /// unless a list of allowed attributes was supplied using the <see cref="AttributesToSerialize"/>
        /// method. For any related resources, attributes are never exposed.
        /// </summary>
        private List<AttrAttribute> GetAttributesToSerialize(IIdentifiable entity)
        {
            var currentResourceType = entity.GetType();
            if (_currentTargetedResource != currentResourceType)
                // We're dealing with a relationship that is being serialized, for which
                // we never want to include any attributes in the payload.
                return new List<AttrAttribute>();

            if (AttributesToSerialize == null)
                return _resourceGraph.GetAttributes(currentResourceType);

            return AttributesToSerialize.ToList();
        }

        /// <summary>
        /// By default, the client serializer does not include any relationships
        /// for entities in the primary data unless explicitly included using
        /// <see cref="RelationshipsToSerialize"/>.
        /// </summary>
        private List<RelationshipAttribute> GetRelationshipsToSerialize(IIdentifiable entity)
        {
            var currentResourceType = entity.GetType();
            // only allow relationship attributes to be serialized if they were set using
            // <see cref="RelationshipsToInclude{T}(Expression{Func{T, dynamic}})"/>
            // and the current <paramref name="entity"/> is a main entry in the primary data.
            if (RelationshipsToSerialize == null)
                return _resourceGraph.GetRelationships(currentResourceType);

            return RelationshipsToSerialize.ToList();
        }
    }
}
