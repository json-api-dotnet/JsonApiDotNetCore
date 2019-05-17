using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    internal class RootNode<TEntity> : IEntityNode where TEntity : class, IIdentifiable
    {
        private HashSet<TEntity> _uniqueEntities;
        public Type EntityType { get; internal set; }
        public IEnumerable UniqueEntities { get { return _uniqueEntities; } }

        public RelationshipProxy[] RelationshipsToNextLayer { get; private set; }
        public IEnumerable<Dictionary<RelationshipProxy, IEnumerable>> RelationshipsToNextLayerByType()
        {
            return RelationshipsToNextLayer.GroupBy(proxy => proxy.DependentType)
                 .Select(gdc => gdc.ToDictionary(p => p, p => UniqueEntities));
        }


        public IRelationshipsFromPreviousLayer RelationshipsFromPreviousLayer { get { return null; } }

        public RootNode(IEnumerable<TEntity> uniqueEntities, RelationshipProxy[] relationships)
        {
            EntityType = typeof(TEntity);
            _uniqueEntities = new HashSet<TEntity>(uniqueEntities);
            RelationshipsToNextLayer = relationships;
        }

        public void UpdateUnique(IEnumerable updated)
        {
            var casted = updated.Cast<TEntity>().ToList();
            var intersected = _uniqueEntities.Intersect(casted, ResourceHookExecutor.Comparer).Cast<TEntity>();
            _uniqueEntities = new HashSet<TEntity>(intersected);
        }

        public void Reassign(IEnumerable source = null)
        {
            var ids = _uniqueEntities.Select(ue => ue.StringId);
            ((List<TEntity>)source).RemoveAll(se => !ids.Contains(se.StringId));
        }
    }

}
