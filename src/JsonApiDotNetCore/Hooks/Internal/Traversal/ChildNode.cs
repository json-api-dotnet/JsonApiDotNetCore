using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Resources;
using RightType = System.Type;

namespace JsonApiDotNetCore.Hooks.Internal.Traversal
{
    /// <summary>
    /// Child node in the tree
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    internal sealed class ChildNode<TResource> : IResourceNode where TResource : class, IIdentifiable
    {
        private readonly IdentifiableComparer _comparer = IdentifiableComparer.Instance;
        /// <inheritdoc />
        public RightType ResourceType { get; }
        /// <inheritdoc />
        public RelationshipProxy[] RelationshipsToNextLayer { get; }
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

        private readonly RelationshipsFromPreviousLayer<TResource> _relationshipsFromPreviousLayer;

        public ChildNode(RelationshipProxy[] nextLayerRelationships, RelationshipsFromPreviousLayer<TResource> prevLayerRelationships)
        {
            ResourceType = typeof(TResource);
            RelationshipsToNextLayer = nextLayerRelationships;
            _relationshipsFromPreviousLayer = prevLayerRelationships;
        }

        /// <inheritdoc />
       public void UpdateUnique(IEnumerable updated)
        {
            List<TResource> cast = updated.Cast<TResource>().ToList();
            foreach (var group in _relationshipsFromPreviousLayer)
            {
                group.RightResources = new HashSet<TResource>(group.RightResources.Intersect(cast, _comparer).Cast<TResource>());
            }
        }

        /// <summary>
        /// Reassignment is done according to provided relationships
        /// </summary>
        public void Reassign(IEnumerable updated = null)
        {
            var unique = (HashSet<TResource>)UniqueResources;
            foreach (var group in _relationshipsFromPreviousLayer)
            {
                var proxy = group.Proxy;
                var leftResources = group.LeftResources;

                foreach (IIdentifiable left in leftResources)
                {
                    var currentValue = proxy.GetValue(left);

                    if (currentValue is IEnumerable<IIdentifiable> relationshipCollection)
                    {
                        var intersection = relationshipCollection.Intersect(unique, _comparer);
                        IEnumerable typedCollection = TypeHelper.CopyToTypedCollection(intersection, relationshipCollection.GetType());
                        proxy.SetValue(left, typedCollection);
                    }
                    else if (currentValue is IIdentifiable relationshipSingle)
                    {
                        if (!unique.Intersect(new HashSet<IIdentifiable> { relationshipSingle }, _comparer).Any())
                        {
                            proxy.SetValue(left, null);
                        }
                    }
                }
            }
        }
    }
}
