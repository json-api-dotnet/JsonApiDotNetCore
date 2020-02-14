using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
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
                return JsonConvert.SerializeObject(Build(entity, new List<AttrAttribute>(), new List<RelationshipAttribute>()));

            _currentTargetedResource = entity.GetType();
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

            _currentTargetedResource = entity.GetType();
            var attributes = GetAttributesToSerialize(entity);
            var relationships = GetRelationshipsToSerialize(entity);
            var document = base.Build(entities, attributes, relationships);
            _currentTargetedResource = null;
            return JsonConvert.SerializeObject(document);
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
