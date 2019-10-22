using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using DependentType = System.Type;

namespace JsonApiDotNetCore.Hooks
{
    /// <summary>
    /// Child node in the tree
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    internal class ChildNode<TResource> : INode where TResource : class, IIdentifiable
    {
        private readonly IdentifiableComparer _comparer = new IdentifiableComparer();
        /// <inheritdoc />
        public DependentType EntityType { get; private set; }
        /// <inheritdoc />
        public RelationshipProxy[] RelationshipsToNextLayer { get; set; }
        /// <inheritdoc />
        public IEnumerable UniqueEntities
        {
            get
            {
                return new HashSet<TResource>(_relationshipsFromPreviousLayer.SelectMany(rfpl => rfpl.DependentEntities));
            }
        }

        /// <inheritdoc />
        public IRelationshipsFromPreviousLayer RelationshipsFromPreviousLayer
        {
            get
            {
                return _relationshipsFromPreviousLayer;
            }
        }

        private readonly RelationshipsFromPreviousLayer<TResource> _relationshipsFromPreviousLayer;

        public ChildNode(RelationshipProxy[] nextLayerRelationships, RelationshipsFromPreviousLayer<TResource> prevLayerRelationships)
        {
            EntityType = typeof(TResource);
            RelationshipsToNextLayer = nextLayerRelationships;
            _relationshipsFromPreviousLayer = prevLayerRelationships;
        }

        /// <inheritdoc />
       public void UpdateUnique(IEnumerable updated)
        {
            List<TResource> casted = updated.Cast<TResource>().ToList();
            foreach (var rpfl in _relationshipsFromPreviousLayer)
            {
                rpfl.DependentEntities = new HashSet<TResource>(rpfl.DependentEntities.Intersect(casted, _comparer).Cast<TResource>());
            }
        }

        /// <summary>
        /// Reassignment is done according to provided relationships
        /// </summary>
        /// <param name="updated"></param>
        public void Reassign(IEnumerable updated = null)
        {
            var unique = (HashSet<TResource>)UniqueEntities;
            foreach (var rfpl in _relationshipsFromPreviousLayer)
            {
                var proxy = rfpl.Proxy;
                var principalEntities = rfpl.PrincipalEntities;

                foreach (IIdentifiable principal in principalEntities)
                {
                    var currentValue = proxy.GetValue(principal);

                    if (currentValue is IEnumerable<IIdentifiable> relationshipCollection)
                    {
                        var newValue = relationshipCollection.Intersect(unique, _comparer).Cast(proxy.DependentType);
                        proxy.SetValue(principal, newValue);
                    }
                    else if (currentValue is IIdentifiable relationshipSingle)
                    {
                        if (!unique.Intersect(new HashSet<IIdentifiable>() { relationshipSingle }, _comparer).Any())
                        {
                            proxy.SetValue(principal, null);
                        }
                    }
                }
            }
        }
    }
}
