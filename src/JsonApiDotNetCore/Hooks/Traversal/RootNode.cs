using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Hooks
{

    /// <summary>
    /// The root node class of the breadth-first-traversal of entity data structures
    /// as performed by the <see cref="ResourceHookExecutor"/>
    /// </summary>
    internal class RootNode<TEntity> : IEntityNode where TEntity : class, IIdentifiable
    {
        private HashSet<TEntity> _uniqueEntities;
        public Type EntityType { get; internal set; }
        public IEnumerable UniqueEntities { get { return _uniqueEntities; } }
        public RelationshipProxy[] RelationshipsToNextLayer { get; private set; }
        public Dictionary<Type, Dictionary<RelationshipProxy, IEnumerable>> PrincipalsToNextLayerByType()
        {
            return RelationshipsToNextLayer
                    .GroupBy(proxy => proxy.DependentType)
                    .ToDictionary(gdc => gdc.Key, gdc => gdc.ToDictionary(p => p, p => UniqueEntities));
        }

        /// <summary>
        /// The current layer entities grouped by affected relationship to the next layer
        /// </summary>
        public Dictionary<RelationshipProxy, IEnumerable> PrincipalsToNextLayer()
        {
            return RelationshipsToNextLayer.ToDictionary(p => p, p => UniqueEntities);
        }

        /// <summary>
        /// The root node does not have a parent layer and therefore does not have any relationships to any previous layer
        /// </summary>
        public IRelationshipsFromPreviousLayer RelationshipsFromPreviousLayer { get { return null; } }

        public RootNode(IEnumerable<TEntity> uniqueEntities, RelationshipProxy[] relationships)
        {
            EntityType = typeof(TEntity);
            _uniqueEntities = new HashSet<TEntity>(uniqueEntities);
            RelationshipsToNextLayer = relationships;
        }

        /// <summary>
        /// Update the internal list of affected entities. 
        /// </summary>
        /// <param name="updated">Updated.</param>
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
