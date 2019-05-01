using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using DependentType = System.Type;

namespace JsonApiDotNetCore.Services
{

    /// <summary>
    /// A helper class that, for a given dependent type, stores all affected
    /// entities grouped by the relationship by which these entities are included
    /// in the traversal tree.
    /// </summary>
    public class RelationshipGroups
    {
        private readonly Dictionary<string, RelationshipGroupEntry> _entitiesByRelationship;
        public RelationshipGroups()
        {
            _entitiesByRelationship = new Dictionary<string, RelationshipGroupEntry>();
        }

        /// <summary>
        /// Add the specified proxy and relatedEntities.
        /// </summary>
        /// <param name="proxy">Proxy.</param>
        /// <param name="relatedEntities">Related entities.</param>
        public void Add(RelationshipProxy proxy, IEnumerable<IIdentifiable> relatedEntities)
        {
            var key = proxy.RelationshipIdentifier;
            if (!_entitiesByRelationship.TryGetValue(key, out var entitiesWithRelationship))
            {
                entitiesWithRelationship = new RelationshipGroupEntry(proxy, new HashSet<IIdentifiable>(relatedEntities));
                _entitiesByRelationship[key] = entitiesWithRelationship;
            }
            else 
            {
                entitiesWithRelationship.Entities.Union(new HashSet<IIdentifiable>(relatedEntities));
            }
        }

        /// <summary>
        /// Entries in this instance.
        /// </summary>
        /// <returns>The entries.</returns>
        public List<RelationshipGroupEntry> Entries()
        {
            return _entitiesByRelationship.Select(kvPair => kvPair.Value).ToList();
        }

    }

    public class RelationshipGroupEntry
    {
        public RelationshipProxy Relationship { get; private set;}
        public HashSet<IIdentifiable> Entities { get; private set; }
        public RelationshipGroupEntry(RelationshipProxy relationship, HashSet<IIdentifiable> entities)
        {
            Relationship = relationship;
            Entities = entities;
        }

    }

    public class RelatedEntitiesInCurrentLayerEntry
    {
        public DependentType DependentType { get; private set }
        public HashSet<IIdentifiable> UniqueSet { get; private set }
        public List<RelationshipGroupEntry> RelationshipGroups { get; private set }

        public RelatedEntitiesInCurrentLayerEntry(
            DependentType _dependentType,
            HashSet<IIdentifiable> _uniqueSet,
            List<RelationshipGroupEntry> _relationshipGroups
        ) {
            DependentType = _dependentType;
            UniqueSet = _uniqueSet;
            RelationshipGroups = _relationshipGroups;
        }
    }

    /// <summary>
    /// A helper class that represents all entities in the current layer that
    /// are being traversed for which hooks will be executed (see IResourceHookExecutor)
    /// </summary>
    public class RelatedEntitiesInCurrentLayer
    {
    
        private readonly Dictionary<DependentType, RelationshipGroups> _relationshipGroups;
        private readonly Dictionary<DependentType, HashSet<IIdentifiable>> _uniqueEntities;
        public RelatedEntitiesInCurrentLayer()
        {
            _relationshipGroups = new Dictionary<DependentType, RelationshipGroups>();
            _uniqueEntities = new Dictionary<DependentType, HashSet<IIdentifiable>>();

        }

        /// <summary>
        /// A boolean that reflects if there are any entities in this layer 
        /// we need to traverse any further for.
        /// </summary>
        /// <returns>A boolean</returns>
        public bool Any()
        {
            return _uniqueEntities.Any();
        }

        /// <summary>
        /// Stores the entities in of the current layer by keeping track of
        /// all the unique entities (for a given dependent type) and keeping
        /// track of the various relationships that are involved with these 
        /// entities, see RelationshipGroups.
        /// </summary>
        /// <param name="relatedEntities">Related entities.</param>
        /// <param name="proxy">Proxy.</param>
        /// <param name="newEntitiesInTree">New entities in tree.</param>
        public void Add(
            IEnumerable<IIdentifiable> relatedEntities,
            RelationshipProxy proxy,
            HashSet<IIdentifiable> newEntitiesInTree
        )
        {
            // the unique set is used to 
            AddToUnique(proxy, newEntitiesInTree);
            AddToRelationshipGroups(proxy, relatedEntities);
        }

        /// <summary>
        /// Entries in this traversal iteration
        /// </summary>
        /// <returns>The entries.</returns>
        public IEnumerable<RelatedEntitiesInCurrentLayerEntry> Entries()
        {
            var dependentTypes = _uniqueEntities.Keys;
            foreach (var type in dependentTypes)
            {
                var uniqueEntities = _uniqueEntities[type];
                var relationshipGroups = _relationshipGroups[type].Entries();
                yield return new RelatedEntitiesInCurrentLayerEntry(type, uniqueEntities, relationshipGroups);
            }
        }

        private void AddToUnique(RelationshipProxy proxy, HashSet<IIdentifiable> newEntitiesInTree)
        {
            if (!proxy.IsContextRelation && !newEntitiesInTree.Any()) return;
            if (!_uniqueEntities.TryGetValue(proxy.TargetType, out HashSet<IIdentifiable> uniqueSet))
            {
                _uniqueEntities[proxy.TargetType] = newEntitiesInTree;
            }
            else
            {
                uniqueSet.UnionWith(newEntitiesInTree);
            }
        }

        private void AddToRelationshipGroups(RelationshipProxy proxy, IEnumerable<IIdentifiable> relatedEntities)
        {
            var key = proxy.TargetType; 
            if (!_relationshipGroups.TryGetValue(key, out RelationshipGroups groups ))
            {
                groups = new RelationshipGroups(); 
                _relationshipGroups[key] = groups;
            }
            groups.Add(proxy, relatedEntities);
        }
    }
}

