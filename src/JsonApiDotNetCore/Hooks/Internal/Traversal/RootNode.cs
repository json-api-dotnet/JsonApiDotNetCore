using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Hooks.Internal.Traversal
{
    /// <summary>
    /// The root node class of the breadth-first-traversal of resource data structures as performed by the <see cref="ResourceHookExecutor" />
    /// </summary>
    internal sealed class RootNode<TResource> : IResourceNode
        where TResource : class, IIdentifiable
    {
        private readonly IdentifiableComparer _comparer = IdentifiableComparer.Instance;
        private readonly IReadOnlyCollection<RelationshipProxy> _allRelationshipsToNextLayer;
        private HashSet<TResource> _uniqueResources;
        public Type ResourceType { get; }
        public IEnumerable UniqueResources => _uniqueResources;
        public IReadOnlyCollection<RelationshipProxy> RelationshipsToNextLayer { get; }

        /// <summary>
        /// The root node does not have a parent layer and therefore does not have any relationships to any previous layer
        /// </summary>
        public IRelationshipsFromPreviousLayer RelationshipsFromPreviousLayer => null;

        public RootNode(IEnumerable<TResource> uniqueResources, IReadOnlyCollection<RelationshipProxy> populatedRelationships,
            IReadOnlyCollection<RelationshipProxy> allRelationships)
        {
            ResourceType = typeof(TResource);
            _uniqueResources = new HashSet<TResource>(uniqueResources);
            RelationshipsToNextLayer = populatedRelationships;
            _allRelationshipsToNextLayer = allRelationships;
        }

        public IDictionary<Type, Dictionary<RelationshipAttribute, IEnumerable>> LeftsToNextLayerByRelationships()
        {
            return _allRelationshipsToNextLayer.GroupBy(proxy => proxy.RightType).ToDictionary(grouping => grouping.Key,
                grouping => grouping.ToDictionary(proxy => proxy.Attribute, _ => UniqueResources));
        }

        /// <summary>
        /// The current layer resources grouped by affected relationship to the next layer
        /// </summary>
        public IDictionary<RelationshipAttribute, IEnumerable> LeftsToNextLayer()
        {
            return RelationshipsToNextLayer.ToDictionary(proxy => proxy.Attribute, _ => UniqueResources);
        }

        /// <summary>
        /// Update the internal list of affected resources.
        /// </summary>
        /// <param name="updated">Updated.</param>
        public void UpdateUnique(IEnumerable updated)
        {
            List<TResource> list = updated.Cast<TResource>().ToList();
            IEnumerable<TResource> intersected = _uniqueResources.Intersect(list, _comparer).Cast<TResource>();
            _uniqueResources = new HashSet<TResource>(intersected);
        }

        public void Reassign(IEnumerable source = null)
        {
            IEnumerable<string> ids = _uniqueResources.Select(ue => ue.StringId);

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
