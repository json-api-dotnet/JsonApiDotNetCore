using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using DependentType = System.Type;
using PrincipalType = System.Type;

namespace JsonApiDotNetCore.Services
{
    public class EntityTreeLayerFactory
    {
        private readonly IHookExecutorHelper _meta;
        private readonly IResourceGraph _graph;
        private readonly Dictionary<Type, HashSet<IIdentifiable>> _processedEntities;


        public EntityTreeLayerFactory(IHookExecutorHelper meta, IResourceGraph graph, Dictionary<Type, HashSet<IIdentifiable>> processedEntities)
        {
            _meta = meta;
            _graph = graph;
            _processedEntities = processedEntities;
        }

        public EntityTreeLayer CreateLayer(EntityTreeLayer currentLayer)
        {
            var nextLayer = new EntityTreeLayer(currentLayer, _meta, _graph, _processedEntities);
            return nextLayer;
        }

        public EntityTreeLayer CreateLayer(IEnumerable<IIdentifiable> currentLayerEntities)
        {
            var layer = new EntityTreeLayer(currentLayerEntities, _meta, _graph, _processedEntities);
            return layer;
        }
    }

    /// <summary>
    /// A helper class that represents all entities in the current layer that
    /// are being traversed for which hooks will be executed (see IResourceHookExecutor)
    /// </summary>
    public class EntityTreeLayer : IEnumerable<NodeInLayer>
    {
        private readonly IHookExecutorHelper _meta;
        private readonly IResourceGraph _graph;

        private readonly Dictionary<PrincipalType, HashSet<IIdentifiable>> _processedEntities;
        private readonly Dictionary<PrincipalType, HashSet<IIdentifiable>> _uniqueEntities;
        private readonly Dictionary<RelationshipAttribute, RelationshipProxy> _relationshipProxies;
        private readonly Dictionary<RelationshipProxy, List<IIdentifiable>> _currentEntitiesByRelationship;
        private readonly Dictionary<RelationshipProxy, List<IIdentifiable>> _previousEntitiesByRelationship;



        public bool IsRootLayer { get; private set; }

        public EntityTreeLayer(
            IEnumerable<IIdentifiable> currentLayerEntities,
            IHookExecutorHelper meta, 
            IResourceGraph graph, 
            Dictionary<Type, HashSet<IIdentifiable>> processedEntities)
        {
            IsRootLayer = true;
            _meta = meta;
            _graph = graph;
            _relationshipProxies = new Dictionary<RelationshipAttribute, RelationshipProxy>();
            _processedEntities = processedEntities;
            _uniqueEntities = new Dictionary<PrincipalType, HashSet<IIdentifiable>>();


            ProcessEntities(currentLayerEntities);
        }


        public EntityTreeLayer(
            EntityTreeLayer currentLayer,
            IHookExecutorHelper meta, 
            IResourceGraph graph, 
            Dictionary<Type, HashSet<IIdentifiable>> processedEntities)
        {
            IsRootLayer = false;
            _meta = meta;
            _graph = graph;
            _relationshipProxies = new Dictionary<RelationshipAttribute, RelationshipProxy>();
            _processedEntities = processedEntities;
            _uniqueEntities = new Dictionary<PrincipalType, HashSet<IIdentifiable>>();

            _currentEntitiesByRelationship = new Dictionary<RelationshipProxy, List<IIdentifiable>>();
            _previousEntitiesByRelationship = new Dictionary<RelationshipProxy, List<IIdentifiable>>();
            ExtractEntities(currentLayer);
        }


        void ExtractEntities(EntityTreeLayer previousLayer)
        {
            var currentLayerEntities = new List<IIdentifiable>();
            foreach (var node in previousLayer)
            {
                var entities = node.UniqueSet;
                var relationships = previousLayer.GetRelationships(node.PrincipalType);
                foreach (IIdentifiable principalEntity in entities)
                {
                    foreach (var proxy in relationships)
                    {
                        var relationshipValue = proxy.GetValue(principalEntity);
                        // skip this relationship if it's not populated
                        if (!proxy.IsContextRelation && relationshipValue == null) continue;
                        if (!(relationshipValue is IEnumerable<IIdentifiable> dependentEntities))
                        {
                            // in the case of a to-one relationship, the assigned value
                            // will not be a list. We therefore first wrap it in a list.
                            var list = TypeHelper.CreateListFor(proxy.DependentType);
                            if (relationshipValue != null) list.Add(relationshipValue);
                            dependentEntities = (IEnumerable<IIdentifiable>)list;
                        }
                        currentLayerEntities.AddRange(dependentEntities);
                        AddToDependentGroups(proxy, dependentEntities);
                        AddToPrincipalGroups(proxy, principalEntity);
                    }
                }
            }
            ProcessEntities(currentLayerEntities);
        }

        public void ProcessEntities(IEnumerable<IIdentifiable> currentLayerEntities)
        {
            var incomingEntitiesByType = currentLayerEntities.GroupBy(e => e.GetType()).ToDictionary(g => g.Key, g => new HashSet<IIdentifiable>(g));

            foreach ( var group in incomingEntitiesByType)
            {
                var principalType = group.Key;
                var incomingEntities = group.Value;
                var uniqueEntities = UniqueInTree(incomingEntities, principalType);

                //if (!uniqueEntities.Any()) continue; TODO check this: setting relation from [ .. ] to null ?

                if (!_uniqueEntities.TryGetValue(principalType, out HashSet<IIdentifiable> entities))
                {
                    _uniqueEntities[principalType] = new HashSet<IIdentifiable> { };
                }
                entities.UnionWith(uniqueEntities);

                var contextEntity = _graph.GetContextEntity(principalType);
                foreach (RelationshipAttribute attr in contextEntity.Relationships)
                {
                    if (!_relationshipProxies.TryGetValue(attr, out RelationshipProxy proxies))
                    {
                        DependentType dependentType = GetDependentTypeFromRelationship(attr);
                        var isContextRelation = false;
                        var proxy = new RelationshipProxy(attr, dependentType,
                                principalType, isContextRelation != null && (bool)isContextRelation);
                    }
                }

            }
        }


        public List<RelationshipProxy> GetRelationships(PrincipalType principal)
        {
            return _relationshipProxies.Select(entry => entry.Value).Where(proxy => proxy.PrincipalType == principal).ToList();
        }

        /// <summary>
        /// Registers the processed entities in the dictionary grouped by type
        /// </summary>
        /// <param name="entities">Entities to register</param>
        /// <param name="entityType">Entity type.</param>
        void RegisterProcessedEntities(IEnumerable<IIdentifiable> entities, Type entityType)
        {
            var processedEntities = GetProcessedEntities(entityType);
            processedEntities.UnionWith(new HashSet<IIdentifiable>(entities));
        }


        /// <summary>
        /// Gets the processed entities for a given type, instantiates the collection if new.
        /// </summary>
        /// <returns>The processed entities.</returns>
        /// <param name="entityType">Entity type.</param>
        HashSet<IIdentifiable> GetProcessedEntities(Type entityType)
        {
            if (!_processedEntities.TryGetValue(entityType, out HashSet<IIdentifiable> processedEntities))
            {
                processedEntities = new HashSet<IIdentifiable>();
                _processedEntities[entityType] = processedEntities;
            }
            return processedEntities;
        }

        /// <summary>
        /// Using the register of processed entities, determines the unique and new
        /// entities with respect to previous iterations.
        /// </summary>
        /// <returns>The in tree.</returns>
        /// <param name="entities">Entities.</param>
        /// <param name="entityType">Entity type.</param>
        HashSet<IIdentifiable> UniqueInTree(IEnumerable<IIdentifiable> entities, Type entityType)
        {
            var newEntities = new HashSet<IIdentifiable>(entities.Except(GetProcessedEntities(entityType)));
            RegisterProcessedEntities(entities, entityType);
            return newEntities;
        }


        /// <summary>
        /// Gets the unique filtered set.
        /// </summary>
        /// <returns>The unique filtered set.</returns>
        /// <param name="proxy">Proxy.</param>
        public HashSet<IIdentifiable> GetUniqueFilteredSet(PrincipalType principalType)
        {
            var match = _uniqueEntities.Where(kvPair => kvPair.Key == principalType);
            return match.Any() ? match.Single().Value : null;
        }

        /// <summary>
        /// Gets all unique entities.
        /// </summary>
        /// <returns>The all unique entities.</returns>
        public List<IIdentifiable> GetAllUniqueEntities()
        {
            return _uniqueEntities.Values.SelectMany(hs => hs).ToList();
        }

        /// <summary>
        /// Gets all dependent types.
        /// </summary>
        /// <returns>The all dependent types.</returns>
        public List<DependentType> GetAllDependentTypes()
        {
            return _uniqueEntities.Keys.ToList();
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




        public IEnumerator<NodeInLayer> GetEnumerator()
        {
            var principalTypes = _uniqueEntities.Keys;
            foreach (PrincipalType principal in principalTypes)
            {
                var relationships = GetRelationships(principal);
                var uniqueEntities = _uniqueEntities[principal];
                var currentLayerByRelationship = _currentEntitiesByRelationship.Where(p => p.Key.DependentType == principal).ToDictionary(p => p.Key, p => p.Value);
                var previousLayerByRelationship = _previousEntitiesByRelationship.Where(p => p.Key.DependentType == principal).ToDictionary(p => p.Key, p => p.Value);
                yield return new NodeInLayer(principal, uniqueEntities, currentLayerByRelationship, previousLayerByRelationship, relationships, IsRootLayer);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        private void AddToDependentGroups(RelationshipProxy proxy, IEnumerable<IIdentifiable> entities)
        {
            AddToRelationshipGroup(_currentEntitiesByRelationship, proxy, entities);

        }

        private void AddToPrincipalGroups(RelationshipProxy proxy, params IIdentifiable[] entities)
        {
            AddToRelationshipGroup(_previousEntitiesByRelationship, proxy, entities);
        }


        private void AddToRelationshipGroup(Dictionary<RelationshipProxy, List<IIdentifiable>> target, RelationshipProxy proxy, IEnumerable<IIdentifiable> newEntities)
        {
            if (!target.TryGetValue(proxy, out List<IIdentifiable> entities))
            {
                entities = new List<IIdentifiable>();
                target[proxy] = entities;
            }
            entities.AddRange(newEntities);
        }

        /// <summary>
        /// Gets the type from relationship attribute. If the attribute is 
        /// HasManyThrough, and the jointable entity is identifiable, then the target
        /// type is the joinentity instead of the righthand side, because hooks might be 
        /// implemented for the jointable entity.
        /// </summary>
        /// <returns>The target type for traversal</returns>
        /// <param name="attr">Relationship attribute</param>
        protected Type GetDependentTypeFromRelationship(RelationshipAttribute attr)
        {
            if (attr is HasManyThroughAttribute throughAttr)
            {
                if (typeof(IIdentifiable).IsAssignableFrom(throughAttr.ThroughType))
                {
                    return throughAttr.ThroughType;
                }
                return attr.Type;
            }
            return attr.Type;
        }


    }
}

