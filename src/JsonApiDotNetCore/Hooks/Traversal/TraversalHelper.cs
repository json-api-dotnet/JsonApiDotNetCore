using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using RightType = System.Type;
using LeftType = System.Type;

namespace JsonApiDotNetCore.Hooks
{
    /// <summary>
    /// A helper class used by the <see cref="ResourceHookExecutor"/> to traverse through
    /// entity data structures (trees), allowing for a breadth-first-traversal
    /// 
    /// It creates nodes for each layer. 
    /// Typically, the first layer is homogeneous (all entities have the same type),
    /// and further nodes can be mixed.
    /// </summary>
    internal class TraversalHelper : ITraversalHelper
    {
        private readonly IdentifiableComparer _comparer = new IdentifiableComparer();
        private readonly IResourceGraph _resourceGraph;
        private readonly ITargetedFields _targetedFields;
        /// <summary>
        /// Keeps track of which entities has already been traversed through, to prevent
        /// infinite loops in eg cyclic data structures.
        /// </summary>
        private Dictionary<RightType, HashSet<IIdentifiable>> _processedEntities;
        /// <summary>
        /// A mapper from <see cref="RelationshipAttribute"/> to <see cref="RelationshipProxy"/>. 
        /// See the latter for more details.
        /// </summary>
        private readonly Dictionary<RelationshipAttribute, RelationshipProxy> _relationshipProxies = new Dictionary<RelationshipAttribute, RelationshipProxy>();
        public TraversalHelper(
            IResourceGraph resourceGraph,
            ITargetedFields targetedFields)
        {
            _targetedFields = targetedFields;
            _resourceGraph = resourceGraph;
        }

        /// <summary>
        /// Creates a root node for breadth-first-traversal. Note that typically, in
        /// JADNC, the root layer will be homogeneous. Also, because it is the first layer,
        /// there can be no relationships to previous layers, only to next layers.
        /// </summary>
        /// <returns>The root node.</returns>
        /// <param name="rootEntities">Root entities.</param>
        /// <typeparam name="TResource">The 1st type parameter.</typeparam>
        public RootNode<TResource> CreateRootNode<TResource>(IEnumerable<TResource> rootEntities) where TResource : class, IIdentifiable
        {
            _processedEntities = new Dictionary<RightType, HashSet<IIdentifiable>>();
            RegisterRelationshipProxies(typeof(TResource));
            var uniqueEntities = ProcessEntities(rootEntities);
            var populatedRelationshipsToNextLayer = GetPopulatedRelationships(typeof(TResource), uniqueEntities.Cast<IIdentifiable>());
            var allRelationshipsFromType = _relationshipProxies.Select(entry => entry.Value).Where(proxy => proxy.LeftType == typeof(TResource)).ToArray();
            return new RootNode<TResource>(uniqueEntities, populatedRelationshipsToNextLayer, allRelationshipsFromType);
        }

        /// <summary>
        /// Create the first layer after the root layer (based on the root node)
        /// </summary>
        /// <returns>The next layer.</returns>
        /// <param name="rootNode">Root node.</param>
        public NodeLayer CreateNextLayer(INode rootNode)
        {
            return CreateNextLayer(new[] { rootNode });
        }

        /// <summary>
        /// Create a next layer from any previous layer
        /// </summary>
        /// <returns>The next layer.</returns>
        /// <param name="nodes">Nodes.</param>
        public NodeLayer CreateNextLayer(IEnumerable<INode> nodes)
        {
            // first extract entities by parsing populated relationships in the entities
            // of previous layer
            (var lefts, var rights) = ExtractEntities(nodes);

            // group them conveniently so we can make ChildNodes of them:
            // there might be several relationship attributes in rights dictionary
            // that point to the same right type. 
            var leftsGrouped = GroupByRightTypeOfRelationship(lefts);

            // convert the groups into child nodes
            var nextNodes = leftsGrouped.Select(entry =>
            {
                var nextNodeType = entry.Key;
                RegisterRelationshipProxies(nextNodeType);

                var populatedRelationships = new List<RelationshipProxy>();
                var relationshipsToPreviousLayer = entry.Value.Select(grouped =>
                {
                    var proxy = grouped.Key;
                    populatedRelationships.AddRange(GetPopulatedRelationships(nextNodeType, rights[proxy]));
                    return CreateRelationshipGroupInstance(nextNodeType, proxy, grouped.Value, rights[proxy]);
                }).ToList();

                return CreateNodeInstance(nextNodeType, populatedRelationships.ToArray(), relationshipsToPreviousLayer);
            }).ToList();

            // wrap the child nodes in a EntityChildLayer
            return new NodeLayer(nextNodes);
        }

        /// <summary>
        /// iterates through the <paramref name="relationships"/> dictionary and groups the values 
        /// by matching right type of the keys (which are relationship attributes)
        /// </summary>
        Dictionary<RightType, List<KeyValuePair<RelationshipProxy, List<IIdentifiable>>>> GroupByRightTypeOfRelationship(Dictionary<RelationshipProxy, List<IIdentifiable>> relationships)
        {
            return relationships.GroupBy(kvp => kvp.Key.RightType).ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());
        }

        /// <summary>
        /// Extracts the entities for the current layer by going through all populated relationships
        /// of the (left entities of the previous layer.
        /// </summary>
        (Dictionary<RelationshipProxy, List<IIdentifiable>>, Dictionary<RelationshipProxy, List<IIdentifiable>>) ExtractEntities(IEnumerable<INode> leftNodes)
        {
            var leftEntitiesGrouped = new Dictionary<RelationshipProxy, List<IIdentifiable>>();  // RelationshipAttr_prevLayer->currentLayer  => prevLayerEntities
            var rightEntitiesGrouped = new Dictionary<RelationshipProxy, List<IIdentifiable>>(); // RelationshipAttr_prevLayer->currentLayer   => currentLayerEntities

            foreach (var node in leftNodes)
            {
                var leftEntities = node.UniqueEntities;
                var relationships = node.RelationshipsToNextLayer;
                foreach (IIdentifiable leftEntity in leftEntities)
                {
                    foreach (var proxy in relationships)
                    {
                        var relationshipValue = proxy.GetValue(leftEntity);
                        // skip this relationship if it's not populated
                        if (!proxy.IsContextRelation && relationshipValue == null) continue;
                        if (!(relationshipValue is IEnumerable rightEntities))
                        {
                            // in the case of a to-one relationship, the assigned value
                            // will not be a list. We therefore first wrap it in a list.
                            var list = TypeHelper.CreateListFor(proxy.RightType);
                            if (relationshipValue != null) list.Add(relationshipValue);
                            rightEntities = list;
                        }

                        var uniqueRightEntities = UniqueInTree(rightEntities.Cast<IIdentifiable>(), proxy.RightType);
                        if (proxy.IsContextRelation || uniqueRightEntities.Any())
                        {
                            AddToRelationshipGroup(rightEntitiesGrouped, proxy, uniqueRightEntities);
                            AddToRelationshipGroup(leftEntitiesGrouped, proxy, new[] { leftEntity });
                        }
                    }
                }
            }

            var processEntitiesMethod = GetType().GetMethod(nameof(ProcessEntities), BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var kvp in rightEntitiesGrouped)
            {
                var type = kvp.Key.RightType;
                var list = kvp.Value.Cast(type);
                processEntitiesMethod.MakeGenericMethod(type).Invoke(this, new object[] { list });
            }

            return (leftEntitiesGrouped, rightEntitiesGrouped);
        }

        /// <summary>
        /// Get all populated relationships known in the current tree traversal from a 
        /// left type to any right type
        /// </summary>
        /// <returns>The relationships.</returns>
        RelationshipProxy[] GetPopulatedRelationships(LeftType leftType, IEnumerable<IIdentifiable> lefts)
        {
            var relationshipsFromLeftToRight = _relationshipProxies.Select(entry => entry.Value).Where(proxy => proxy.LeftType == leftType);
            return relationshipsFromLeftToRight.Where(proxy => proxy.IsContextRelation || lefts.Any(p => proxy.GetValue(p) != null)).ToArray();
        }

        /// <summary>
        /// Registers the entities as "seen" in the tree traversal, extracts any new <see cref="RelationshipProxy"/>s from it.
        /// </summary>
        /// <returns>The entities.</returns>
        /// <param name="incomingEntities">Incoming entities.</param>
        /// <typeparam name="TResource">The 1st type parameter.</typeparam>
        HashSet<TResource> ProcessEntities<TResource>(IEnumerable<TResource> incomingEntities) where TResource : class, IIdentifiable
        {
            Type type = typeof(TResource);
            var newEntities = UniqueInTree(incomingEntities, type);
            RegisterProcessedEntities(newEntities, type);
            return newEntities;
        }

        /// <summary>
        /// Parses all relationships from <paramref name="type"/> to
        /// other models in the resource resourceGraphs by constructing RelationshipProxies .
        /// </summary>
        /// <param name="type">The type to parse</param>
        void RegisterRelationshipProxies(RightType type)
        {
            foreach (RelationshipAttribute attr in _resourceGraph.GetRelationships(type))
            {
                if (!attr.CanInclude) continue;
                if (!_relationshipProxies.TryGetValue(attr, out RelationshipProxy proxies))
                {
                    RightType rightType = GetRightTypeFromRelationship(attr);
                    bool isContextRelation = false;
                    var relationshipsToUpdate = _targetedFields.Relationships;
                    if (relationshipsToUpdate != null) isContextRelation = relationshipsToUpdate.Contains(attr);
                    var proxy = new RelationshipProxy(attr, rightType, isContextRelation);
                    _relationshipProxies[attr] = proxy;
                }
            }
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
        HashSet<TResource> UniqueInTree<TResource>(IEnumerable<TResource> entities, Type entityType) where TResource : class, IIdentifiable
        {
            var newEntities = entities.Except(GetProcessedEntities(entityType), _comparer).Cast<TResource>();
            return new HashSet<TResource>(newEntities);
        }

        /// <summary>
        /// Gets the type from relationship attribute. If the attribute is 
        /// HasManyThrough, and the join table entity is identifiable, then the target
        /// type is the join entity instead of the right-hand side, because hooks might be 
        /// implemented for the join table entity.
        /// </summary>
        /// <returns>The target type for traversal</returns>
        /// <param name="attr">Relationship attribute</param>
        RightType GetRightTypeFromRelationship(RelationshipAttribute attr)
        {
            if (attr is HasManyThroughAttribute throughAttr && throughAttr.ThroughType.Inherits(typeof(IIdentifiable)))
            {
                return throughAttr.ThroughType;
            }
            return attr.RightType;
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

        /// <summary>
        /// Reflective helper method to create an instance of <see cref="ChildNode{TResource}"/>;
        /// </summary>
        INode CreateNodeInstance(RightType nodeType, RelationshipProxy[] relationshipsToNext, IEnumerable<IRelationshipGroup> relationshipsFromPrev)
        {
            IRelationshipsFromPreviousLayer prev = CreateRelationshipsFromInstance(nodeType, relationshipsFromPrev);
            return (INode)TypeHelper.CreateInstanceOfOpenType(typeof(ChildNode<>), nodeType, new object[] { relationshipsToNext, prev });
        }

        /// <summary>
        /// Reflective helper method to create an instance of <see cref="RelationshipsFromPreviousLayer{TRight}"/>;
        /// </summary>
        IRelationshipsFromPreviousLayer CreateRelationshipsFromInstance(RightType nodeType, IEnumerable<IRelationshipGroup> relationshipsFromPrev)
        {
            var cast = relationshipsFromPrev.Cast(relationshipsFromPrev.First().GetType());
            return (IRelationshipsFromPreviousLayer)TypeHelper.CreateInstanceOfOpenType(typeof(RelationshipsFromPreviousLayer<>), nodeType, new object[] { cast });
        }

        /// <summary>
        /// Reflective helper method to create an instance of <see cref="RelationshipGroup{TRight}"/>;
        /// </summary>
        IRelationshipGroup CreateRelationshipGroupInstance(Type thisLayerType, RelationshipProxy proxy, List<IIdentifiable> leftEntities, List<IIdentifiable> rightEntities)
        {
            var rightEntitiesHashed = TypeHelper.CreateInstanceOfOpenType(typeof(HashSet<>), thisLayerType, rightEntities.Cast(thisLayerType));
            return (IRelationshipGroup)TypeHelper.CreateInstanceOfOpenType(typeof(RelationshipGroup<>),
                thisLayerType,
                new object[] { proxy, new HashSet<IIdentifiable>(leftEntities), rightEntitiesHashed });
        }
    }

    /// <summary>
    /// A helper class that represents all entities in the current layer that
    /// are being traversed for which hooks will be executed (see IResourceHookExecutor)
    /// </summary>
    internal class NodeLayer : IEnumerable<INode>
    {
        readonly List<INode> _collection;

        public bool AnyEntities()
        {
            return _collection.Any(n => n.UniqueEntities.Cast<IIdentifiable>().Any());
        }

        public NodeLayer(List<INode> nodes)
        {
            _collection = nodes;
        }

        public IEnumerator<INode> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

