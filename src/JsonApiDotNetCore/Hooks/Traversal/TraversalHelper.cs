using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using DependentType = System.Type;
using PrincipalType = System.Type;

namespace JsonApiDotNetCore.Services
{
    internal class TraversalHelper
    {
        private readonly IResourceGraph _graph;
        private readonly IJsonApiContext _context;
        private Dictionary<DependentType, HashSet<IIdentifiable>> _processedEntities;
        private readonly Dictionary<RelationshipAttribute, RelationshipProxy> RelationshipProxies = new Dictionary<RelationshipAttribute, RelationshipProxy>();

        public TraversalHelper(
            IResourceGraph graph,
            IJsonApiContext context)
        {
            _context = context;
            _graph = graph;
        }

        public RootNode<TEntity> CreateRootNode<TEntity>(IEnumerable<TEntity> rootEntities) where TEntity : class, IIdentifiable
        {
            _processedEntities = new Dictionary<DependentType, HashSet<IIdentifiable>>();
            var uniqueEntities = ProcessEntities(rootEntities);
            var relationshipsToNextLayer = GetRelationships(typeof(TEntity));
            return new RootNode<TEntity>(uniqueEntities, relationshipsToNextLayer);
        }

        public EntityChildLayer CreateNextLayer(IEntityNode rootNode)
        {
            return CreateNextLayer(new IEntityNode[] { rootNode });
        }

        public EntityChildLayer CreateNextLayer(IEnumerable<IEntityNode> nodes)
        {
            (var dependents, var principals) = ExtractEntities(nodes);
            var dependentsGrouped = GroupByDependentTypeOfRelationship(dependents);

            var nextNodes = dependentsGrouped.Select(entry =>
            {
                var nextNodeType = entry.Key;
                var relationshipsToPreviousLayer = entry.Value.Select(grouped =>
                {
                    var proxy = grouped.Key;
                    return CreateRelationsipGroupInstance(nextNodeType, proxy, grouped.Value, principals[proxy]);
                }).ToList();


                return CreateNodeInstance(nextNodeType, GetRelationships(nextNodeType), relationshipsToPreviousLayer);
            }).ToList();

            return new EntityChildLayer(nextNodes);
        }

        IEntityNode CreateNodeInstance(DependentType nodeType, RelationshipProxy[] relationshipsToNext, IEnumerable<IRelationshipGroup> relationshipsFromPrev)
        {
            IRelationshipsFromPreviousLayer prev = CreateRelationshipsFromInstance(nodeType, relationshipsFromPrev);
            return (IEntityNode)TypeHelper.CreateInstanceOfOpenType(typeof(ChildNode<>), nodeType, new object[] { relationshipsToNext, prev });
        }

        IRelationshipsFromPreviousLayer CreateRelationshipsFromInstance(DependentType nodeType, IEnumerable<IRelationshipGroup> relationshipsFromPrev)
        {
            var casted = TypeHelper.ConvertCollection(relationshipsFromPrev, relationshipsFromPrev.First().GetType());
            return (IRelationshipsFromPreviousLayer)TypeHelper.CreateInstanceOfOpenType(typeof(RelationshipsFromPreviousLayer<>), nodeType, new object[] { casted });
        }

        IRelationshipGroup CreateRelationsipGroupInstance(Type thisLayerType, RelationshipProxy proxy, List<IIdentifiable> principalEntities, List<IIdentifiable> dependentEntities)
        {
            var dependentEntitiesHashed = TypeHelper.CreateInstanceOfOpenType(typeof(HashSet<>), thisLayerType, TypeHelper.ConvertCollection(dependentEntities, thisLayerType));
            return (IRelationshipGroup)TypeHelper.CreateInstanceOfOpenType(typeof(RelationshipGroup<>),
                thisLayerType,
                new object[] { proxy, new HashSet<IIdentifiable>(principalEntities), dependentEntitiesHashed });
        }

        Dictionary<DependentType, List<KeyValuePair<RelationshipProxy, List<IIdentifiable>>>> GroupByDependentTypeOfRelationship(Dictionary<RelationshipProxy, List<IIdentifiable>> relationships)
        {
            return relationships.GroupBy(kvp => kvp.Key.DependentType).ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());
        }

        (Dictionary<RelationshipProxy, List<IIdentifiable>>, Dictionary<RelationshipProxy, List<IIdentifiable>>) ExtractEntities(IEnumerable<IEntityNode> principalNodes)
        {
            var currentLayerEntities = new List<IIdentifiable>();
            var principalsGrouped = new Dictionary<RelationshipProxy, List<IIdentifiable>>();
            var dependentsGrouped = new Dictionary<RelationshipProxy, List<IIdentifiable>>();

            foreach (var node in principalNodes)
            {
                var principalEntities = node.UniqueEntities;
                var relationships = node.RelationshipsToNextLayer;
                foreach (IIdentifiable principalEntity in principalEntities)
                {
                    foreach (var proxy in relationships)
                    {
                        var relationshipValue = proxy.GetValue(principalEntity);
                        // skip this relationship if it's not populated
                        if (!proxy.IsContextRelation && relationshipValue == null) continue;
                        if (!(relationshipValue is IEnumerable dependentEntities))
                        {
                            // in the case of a to-one relationship, the assigned value
                            // will not be a list. We therefore first wrap it in a list.
                            var list = TypeHelper.CreateListFor(proxy.DependentType);
                            if (relationshipValue != null) list.Add(relationshipValue);
                            dependentEntities = list;
                        }

                        var newDependentEntities = UniqueInTree(dependentEntities.Cast<IIdentifiable>(), proxy.DependentType);
                        if (proxy.IsContextRelation || newDependentEntities.Any())
                        {
                            currentLayerEntities.AddRange(newDependentEntities);
                            AddToRelationshipGroup(dependentsGrouped, proxy, newDependentEntities); // TODO check if this needs to be newDependentEntities or just dependentEntities
                            AddToRelationshipGroup(principalsGrouped, proxy, new IIdentifiable[] { principalEntity });
                        }
                    }
                }
            }

            var processEntities = GetType().GetMethod(nameof(ProcessEntities), BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var kvp in dependentsGrouped)
            {
                var type = kvp.Key.DependentType;
                var list = TypeHelper.ConvertCollection(kvp.Value, type);
                processEntities.MakeGenericMethod(type).Invoke(this, new object[] { list });
            }

            return (principalsGrouped, dependentsGrouped);
        }

        RelationshipProxy[] GetRelationships(PrincipalType principal)
        {
            return RelationshipProxies.Select(entry => entry.Value).Where(proxy => proxy.PrincipalType == principal).ToArray();
        }

        HashSet<TEntity> ProcessEntities<TEntity>(IEnumerable<TEntity> incomingEntities) where TEntity : class, IIdentifiable
        {
            Type type = typeof(TEntity);
            var newEntities = UniqueInTree(incomingEntities, type);
            RegisterProcessedEntities(newEntities, type);

            var contextEntity = _graph.GetContextEntity(type);
            foreach (RelationshipAttribute attr in contextEntity.Relationships)
            {
                if (!RelationshipProxies.TryGetValue(attr, out RelationshipProxy proxies))
                {
                    DependentType dependentType = GetDependentTypeFromRelationship(attr);
                    var isContextRelation = _context.RelationshipsToUpdate?.ContainsKey(attr);
                    var proxy = new RelationshipProxy(attr, dependentType, isContextRelation != null && (bool)isContextRelation);
                    RelationshipProxies[attr] = proxy;
                }
            }
            return newEntities;
        }

        private DependentType GetTypeOfList<TEntity>(IEnumerable<TEntity> incomingEntities) where TEntity : class, IIdentifiable
        {
            if (typeof(TEntity) == typeof(IIdentifiable))
            {
                return incomingEntities.First().GetType();
            }
            return typeof(TEntity);
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
        HashSet<TEntity> UniqueInTree<TEntity>(IEnumerable<TEntity> entities, Type entityType) where TEntity : class, IIdentifiable
        {
            var newEntities = entities.Except(GetProcessedEntities(entityType), ResourceHookExecutor.Comparer).Cast<TEntity>();
            return new HashSet<TEntity>(newEntities);
        }

        /// <summary>
        /// Gets the type from relationship attribute. If the attribute is 
        /// HasManyThrough, and the jointable entity is identifiable, then the target
        /// type is the joinentity instead of the righthand side, because hooks might be 
        /// implemented for the jointable entity.
        /// </summary>
        /// <returns>The target type for traversal</returns>
        /// <param name="attr">Relationship attribute</param>
        DependentType GetDependentTypeFromRelationship(RelationshipAttribute attr)
        {
            if (attr is HasManyThroughAttribute throughAttr && throughAttr.ThroughType.Inherits(typeof(IIdentifiable)))
            {
                return throughAttr.ThroughType;
            }
            return attr.DependentType;
        }

        void AddToRelationshipGroup(Dictionary<RelationshipProxy, List<IIdentifiable>> target, RelationshipProxy proxy, IEnumerable<IIdentifiable> newEntities)
        {
            if (!target.TryGetValue(proxy, out List<IIdentifiable> entities))
            {
                entities = new List<IIdentifiable>();
                target[proxy] = entities;
            }
            entities.AddRange(newEntities);
        }
    }

    /// <summary>
    /// A helper class that represents all entities in the current layer that
    /// are being traversed for which hooks will be executed (see IResourceHookExecutor)
    /// </summary>
    internal class EntityChildLayer : IEnumerable<IEntityNode>
    {
        readonly List<IEntityNode> _collection;

        public bool AnyEntities()
        {
            return _collection.Any(n => n.UniqueEntities.Cast<IIdentifiable>().Any());
        }

        public EntityChildLayer(List<IEntityNode> nodes)
        {
            _collection = nodes;
        }

        public IEnumerator<IEntityNode> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

