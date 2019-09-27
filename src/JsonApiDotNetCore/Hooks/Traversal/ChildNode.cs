using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models;
using DependentType = System.Type;

namespace JsonApiDotNetCore.Hooks
{
    /// <summary>
    /// Child node in the tree
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    internal class ChildNode<TEntity> : INode where TEntity : class, IIdentifiable
    {
        /// <inheritdoc />
        public DependentType EntityType { get; private set; }
        /// <inheritdoc />
        public RelationshipProxy[] RelationshipsToNextLayer { get; set; }
        /// <inheritdoc />
        public IEnumerable UniqueEntities
        {
            get
            {
                return new HashSet<TEntity>(_relationshipsFromPreviousLayer.SelectMany(rfpl => rfpl.DependentEntities));
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

        private readonly RelationshipsFromPreviousLayer<TEntity> _relationshipsFromPreviousLayer;

        public ChildNode(RelationshipProxy[] nextLayerRelationships, RelationshipsFromPreviousLayer<TEntity> prevLayerRelationships)
        {
            EntityType = typeof(TEntity);
            RelationshipsToNextLayer = nextLayerRelationships;
            _relationshipsFromPreviousLayer = prevLayerRelationships;
        }

        /// <inheritdoc />
       public void UpdateUnique(IEnumerable updated)
        {
            List<TEntity> casted = updated.Cast<TEntity>().ToList();
            foreach (var rpfl in _relationshipsFromPreviousLayer)
            {
                rpfl.DependentEntities = new HashSet<TEntity>(rpfl.DependentEntities.Intersect(casted, ResourceHookExecutor.Comparer).Cast<TEntity>());
            }
        }

        /// <summary>
        /// Reassignment is done according to provided relationships
        /// </summary>
        /// <param name="updated"></param>
        public void Reassign(IEnumerable updated = null)
        {
            var unique = (HashSet<TEntity>)UniqueEntities;
            foreach (var rfpl in _relationshipsFromPreviousLayer)
            {
                var proxy = rfpl.Proxy;
                var principalEntities = rfpl.PrincipalEntities;

                foreach (IIdentifiable principal in principalEntities)
                {
                    var currentValue = proxy.GetValue(principal);

                    if (currentValue is IEnumerable<IIdentifiable> relationshipCollection)
                    {
                        var newValue = relationshipCollection.Intersect(unique, ResourceHookExecutor.Comparer).Cast(proxy.DependentType);
                        proxy.SetValue(principal, newValue);
                    }
                    else if (currentValue is IIdentifiable relationshipSingle)
                    {
                        if (!unique.Intersect(new HashSet<IIdentifiable>() { relationshipSingle }, ResourceHookExecutor.Comparer).Any())
                        {
                            proxy.SetValue(principal, null);
                        }
                    }
                }
            }
        }
    }
}
