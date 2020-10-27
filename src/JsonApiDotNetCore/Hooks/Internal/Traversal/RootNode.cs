using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Hooks.Internal.Traversal
{
    /// <summary>
    /// The root node class of the breadth-first-traversal of resource data structures
    /// as performed by the <see cref="ResourceHookExecutor"/>
    /// </summary>
    internal sealed class RootNode<TResource> : IResourceNode where TResource : class, IIdentifiable
    {
        private readonly IdentifiableComparer _comparer = IdentifiableComparer.Instance;
        private readonly RelationshipProxy[] _allRelationshipsToNextLayer;
        private HashSet<TResource> _uniqueResources;
        public Type ResourceType { get; }
        public IEnumerable UniqueResources => _uniqueResources;
        public RelationshipProxy[] RelationshipsToNextLayer { get; }

        public Dictionary<Type, Dictionary<RelationshipAttribute, IEnumerable>> LeftsToNextLayerByRelationships()
        {
            return _allRelationshipsToNextLayer
                    .GroupBy(proxy => proxy.RightType)
                    .ToDictionary(gdc => gdc.Key, gdc => gdc.ToDictionary(p => p.Attribute, p => UniqueResources));
        }

        /// <summary>
        /// The current layer resources grouped by affected relationship to the next layer
        /// </summary>
        public Dictionary<RelationshipAttribute, IEnumerable> LeftsToNextLayer()
        {
            return RelationshipsToNextLayer.ToDictionary(p => p.Attribute, p => UniqueResources);
        }

        /// <summary>
        /// The root node does not have a parent layer and therefore does not have any relationships to any previous layer
        /// </summary>
        public IRelationshipsFromPreviousLayer RelationshipsFromPreviousLayer => null;

        public RootNode(IEnumerable<TResource> uniqueResources, RelationshipProxy[] populatedRelationships, RelationshipProxy[] allRelationships)
        {
            ResourceType = typeof(TResource);
            _uniqueResources = new HashSet<TResource>(uniqueResources);
            RelationshipsToNextLayer = populatedRelationships;
            _allRelationshipsToNextLayer = allRelationships;
        }

        /// <summary>
        /// Update the internal list of affected resources. 
        /// </summary>
        /// <param name="updated">Updated.</param>
        public void UpdateUnique(IEnumerable updated)
        {
            var cast = updated.Cast<TResource>().ToList();
            var intersected = _uniqueResources.Intersect(cast, _comparer).Cast<TResource>();
            _uniqueResources = new HashSet<TResource>(intersected);
        }

        public void Reassign(IEnumerable source = null)
        {
            var ids = _uniqueResources.Select(ue => ue.StringId);

            if (source is HashSet<TResource> hashSet)
            {
                hashSet.RemoveWhere(se => !ids.Contains(se.StringId));
            }
            else if (source is List<TResource> list)
            {
                list.RemoveAll(se => !ids.Contains(se.StringId));
            }
            else if (source != null)
            {
                throw new NotSupportedException($"Unsupported collection type '{source.GetType()}'.");
            }
        }
    }
}
