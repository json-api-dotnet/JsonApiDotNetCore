using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using DependentType = System.Type;

namespace JsonApiDotNetCore.Hooks
{
    internal class ChildNode<TEntity> : IEntityNode where TEntity : class, IIdentifiable
    {

        public DependentType EntityType { get; private set; }
        public RelationshipProxy[] RelationshipsToNextLayer { get; set; }
        public IEnumerable UniqueEntities
        {
            get
            {
                return new HashSet<TEntity>(_relationshipsFromPreviousLayer.SelectMany(rfpl => rfpl.DependentEntities));
            }
        }
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

        public void UpdateUnique(IEnumerable updated)
        {
            List<TEntity> casted = updated.Cast<TEntity>().ToList();
            foreach (var rpfl in _relationshipsFromPreviousLayer)
            {
                rpfl.DependentEntities = new HashSet<TEntity>(rpfl.DependentEntities.Intersect(casted, ResourceHookExecutor.Comparer).Cast<TEntity>());
            }
        }

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
                        var newValue = TypeHelper.ConvertCollection(relationshipCollection.Intersect(unique, ResourceHookExecutor.Comparer), proxy.DependentType);
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
