using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Resources;
using RightType = System.Type;

namespace JsonApiDotNetCore.Hooks.Internal.Traversal
{
    internal abstract class ChildNode
    {
        protected static readonly CollectionConverter CollectionConverter = new CollectionConverter();
    }

    /// <summary>
    /// Child node in the tree
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    internal sealed class ChildNode<TResource> : ChildNode, IResourceNode
        where TResource : class, IIdentifiable
    {
        private readonly IdentifiableComparer _comparer = IdentifiableComparer.Instance;
        private readonly RelationshipsFromPreviousLayer<TResource> _relationshipsFromPreviousLayer;

        /// <inheritdoc />
        public RightType ResourceType { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<RelationshipProxy> RelationshipsToNextLayer { get; }

        /// <inheritdoc />
        public IEnumerable UniqueResources
        {
            get
            {
                return new HashSet<TResource>(_relationshipsFromPreviousLayer.SelectMany(relationshipGroup => relationshipGroup.RightResources));
            }
        }

        /// <inheritdoc />
        public IRelationshipsFromPreviousLayer RelationshipsFromPreviousLayer => _relationshipsFromPreviousLayer;

        public ChildNode(IReadOnlyCollection<RelationshipProxy> nextLayerRelationships, RelationshipsFromPreviousLayer<TResource> prevLayerRelationships)
        {
            ResourceType = typeof(TResource);
            RelationshipsToNextLayer = nextLayerRelationships;
            _relationshipsFromPreviousLayer = prevLayerRelationships;
        }

        /// <inheritdoc />
        public void UpdateUnique(IEnumerable updated)
        {
            List<TResource> list = updated.Cast<TResource>().ToList();

            foreach (RelationshipGroup<TResource> group in _relationshipsFromPreviousLayer)
            {
                group.RightResources = new HashSet<TResource>(group.RightResources.Intersect(list, _comparer).Cast<TResource>());
            }
        }

        /// <summary>
        /// Reassignment is done according to provided relationships
        /// </summary>
        public void Reassign(IEnumerable updated = null)
        {
            var unique = (HashSet<TResource>)UniqueResources;

            foreach (RelationshipGroup<TResource> group in _relationshipsFromPreviousLayer)
            {
                RelationshipProxy proxy = group.Proxy;
                HashSet<IIdentifiable> leftResources = group.LeftResources;

                Reassign(leftResources, proxy, unique);
            }
        }

        private void Reassign(IEnumerable<IIdentifiable> leftResources, RelationshipProxy proxy, HashSet<TResource> unique)
        {
            foreach (IIdentifiable left in leftResources)
            {
                object currentValue = proxy.GetValue(left);

                if (currentValue is IEnumerable<IIdentifiable> relationshipCollection)
                {
                    IEnumerable<IIdentifiable> intersection = relationshipCollection.Intersect(unique, _comparer);
                    IEnumerable typedCollection = CollectionConverter.CopyToTypedCollection(intersection, relationshipCollection.GetType());
                    proxy.SetValue(left, typedCollection);
                }
                else if (currentValue is IIdentifiable relationshipSingle)
                {
                    if (!unique.Intersect(new HashSet<IIdentifiable>
                    {
                        relationshipSingle
                    }, _comparer).Any())
                    {
                        proxy.SetValue(left, null);
                    }
                }
            }
        }
    }
}
