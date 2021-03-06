using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using RightType = System.Type;
using LeftType = System.Type;

namespace JsonApiDotNetCore.Hooks.Internal.Traversal
{
    /// <summary>
    /// A helper class used by the <see cref="ResourceHookExecutor" /> to traverse through resource data structures (trees), allowing for a
    /// breadth-first-traversal It creates nodes for each layer. Typically, the first layer is homogeneous (all resources have the same type), and further
    /// nodes can be mixed.
    /// </summary>
    internal sealed class TraversalHelper : ITraversalHelper
    {
        private readonly IdentifiableComparer _comparer = IdentifiableComparer.Instance;
        private readonly IResourceGraph _resourceGraph;
        private readonly ITargetedFields _targetedFields;

        /// <summary>
        /// A mapper from <see cref="RelationshipAttribute" /> to <see cref="RelationshipProxy" />. See the latter for more details.
        /// </summary>
        private readonly Dictionary<RelationshipAttribute, RelationshipProxy> _relationshipProxies = new Dictionary<RelationshipAttribute, RelationshipProxy>();

        /// <summary>
        /// Keeps track of which resources has already been traversed through, to prevent infinite loops in eg cyclic data structures.
        /// </summary>
        private Dictionary<RightType, HashSet<IIdentifiable>> _processedResources;

        public TraversalHelper(IResourceGraph resourceGraph, ITargetedFields targetedFields)
        {
            _targetedFields = targetedFields;
            _resourceGraph = resourceGraph;
        }

        /// <summary>
        /// Creates a root node for breadth-first-traversal. Note that typically, in JsonApiDotNetCore, the root layer will be homogeneous. Also, because it is
        /// the first layer, there can be no relationships to previous layers, only to next layers.
        /// </summary>
        /// <returns>
        /// The root node.
        /// </returns>
        /// <param name="rootResources">
        /// Root resources.
        /// </param>
        /// <typeparam name="TResource">
        /// The 1st type parameter.
        /// </typeparam>
        public RootNode<TResource> CreateRootNode<TResource>(IEnumerable<TResource> rootResources)
            where TResource : class, IIdentifiable
        {
            _processedResources = new Dictionary<RightType, HashSet<IIdentifiable>>();
            RegisterRelationshipProxies(typeof(TResource));
            HashSet<TResource> uniqueResources = ProcessResources(rootResources);
            RelationshipProxy[] populatedRelationshipsToNextLayer = GetPopulatedRelationships(typeof(TResource), uniqueResources);

            RelationshipProxy[] allRelationshipsFromType =
                _relationshipProxies.Select(entry => entry.Value).Where(proxy => proxy.LeftType == typeof(TResource)).ToArray();

            return new RootNode<TResource>(uniqueResources, populatedRelationshipsToNextLayer, allRelationshipsFromType);
        }

        /// <summary>
        /// Create the first layer after the root layer (based on the root node)
        /// </summary>
        /// <returns>
        /// The next layer.
        /// </returns>
        /// <param name="rootNode">
        /// Root node.
        /// </param>
        public NodeLayer CreateNextLayer(IResourceNode rootNode)
        {
            return CreateNextLayer(rootNode.AsEnumerable());
        }

        /// <summary>
        /// Create a next layer from any previous layer
        /// </summary>
        /// <returns>
        /// The next layer.
        /// </returns>
        /// <param name="nodes">Nodes.</param>
        public NodeLayer CreateNextLayer(IEnumerable<IResourceNode> nodes)
        {
            // first extract resources by parsing populated relationships in the resources
            // of previous layer
            (Dictionary<RelationshipProxy, List<IIdentifiable>> lefts, Dictionary<RelationshipProxy, List<IIdentifiable>> rights) = ExtractResources(nodes);

            // group them conveniently so we can make ChildNodes of them:
            // there might be several relationship attributes in rights dictionary
            // that point to the same right type.
            Dictionary<RightType, List<KeyValuePair<RelationshipProxy, List<IIdentifiable>>>> leftsGrouped = GroupByRightTypeOfRelationship(lefts);

            // convert the groups into child nodes
            List<IResourceNode> nextNodes = leftsGrouped.Select(entry =>
            {
                RightType nextNodeType = entry.Key;
                RegisterRelationshipProxies(nextNodeType);

                var populatedRelationships = new List<RelationshipProxy>();

                List<IRelationshipGroup> relationshipsToPreviousLayer = entry.Value.Select(grouped =>
                {
                    RelationshipProxy proxy = grouped.Key;
                    populatedRelationships.AddRange(GetPopulatedRelationships(nextNodeType, rights[proxy]));
                    return CreateRelationshipGroupInstance(nextNodeType, proxy, grouped.Value, rights[proxy]);
                }).ToList();

                return CreateNodeInstance(nextNodeType, populatedRelationships.ToArray(), relationshipsToPreviousLayer);
            }).ToList();

            return new NodeLayer(nextNodes);
        }

        /// <summary>
        /// iterates through the <paramref name="relationships" /> dictionary and groups the values by matching right type of the keys (which are relationship
        /// attributes)
        /// </summary>
        private Dictionary<RightType, List<KeyValuePair<RelationshipProxy, List<IIdentifiable>>>> GroupByRightTypeOfRelationship(
            Dictionary<RelationshipProxy, List<IIdentifiable>> relationships)
        {
            return relationships.GroupBy(kvp => kvp.Key.RightType).ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());
        }

        /// <summary>
        /// Extracts the resources for the current layer by going through all populated relationships of the (left resources of the previous layer.
        /// </summary>
        private (Dictionary<RelationshipProxy, List<IIdentifiable>>, Dictionary<RelationshipProxy, List<IIdentifiable>>) ExtractResources(
            IEnumerable<IResourceNode> leftNodes)
        {
            // RelationshipAttr_prevLayer->currentLayer  => prevLayerResources
            var leftResourcesGrouped = new Dictionary<RelationshipProxy, List<IIdentifiable>>();

            // RelationshipAttr_prevLayer->currentLayer  => currentLayerResources
            var rightResourcesGrouped = new Dictionary<RelationshipProxy, List<IIdentifiable>>();

            foreach (IResourceNode node in leftNodes)
            {
                IEnumerable leftResources = node.UniqueResources;
                RelationshipProxy[] relationships = node.RelationshipsToNextLayer;

                ExtractLeftResources(leftResources, relationships, rightResourcesGrouped, leftResourcesGrouped);
            }

            MethodInfo processResourcesMethod = GetType().GetMethod(nameof(ProcessResources), BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (KeyValuePair<RelationshipProxy, List<IIdentifiable>> kvp in rightResourcesGrouped)
            {
                RightType type = kvp.Key.RightType;
                IList list = TypeHelper.CopyToList(kvp.Value, type);
                processResourcesMethod!.MakeGenericMethod(type).Invoke(this, ArrayFactory.Create<object>(list));
            }

            return (leftResourcesGrouped, rightResourcesGrouped);
        }

        private void ExtractLeftResources(IEnumerable leftResources, RelationshipProxy[] relationships,
            Dictionary<RelationshipProxy, List<IIdentifiable>> rightResourcesGrouped, Dictionary<RelationshipProxy, List<IIdentifiable>> leftResourcesGrouped)
        {
            foreach (IIdentifiable leftResource in leftResources)
            {
                ExtractLeftResource(leftResource, relationships, rightResourcesGrouped, leftResourcesGrouped);
            }
        }

        private void ExtractLeftResource(IIdentifiable leftResource, RelationshipProxy[] relationships,
            Dictionary<RelationshipProxy, List<IIdentifiable>> rightResourcesGrouped, Dictionary<RelationshipProxy, List<IIdentifiable>> leftResourcesGrouped)
        {
            foreach (RelationshipProxy proxy in relationships)
            {
                object relationshipValue = proxy.GetValue(leftResource);

                // skip this relationship if it's not populated
                if (!proxy.IsContextRelation && relationshipValue == null)
                {
                    continue;
                }

                if (!(relationshipValue is IEnumerable rightResources))
                {
                    // in the case of a to-one relationship, the assigned value
                    // will not be a list. We therefore first wrap it in a list.
                    IList list = TypeHelper.CreateListFor(proxy.RightType);

                    if (relationshipValue != null)
                    {
                        list.Add(relationshipValue);
                    }

                    rightResources = list;
                }

                HashSet<IIdentifiable> uniqueRightResources = UniqueInTree(rightResources.Cast<IIdentifiable>(), proxy.RightType);

                if (proxy.IsContextRelation || uniqueRightResources.Any())
                {
                    AddToRelationshipGroup(rightResourcesGrouped, proxy, uniqueRightResources);
                    AddToRelationshipGroup(leftResourcesGrouped, proxy, leftResource.AsEnumerable());
                }
            }
        }

        /// <summary>
        /// Get all populated relationships known in the current tree traversal from a left type to any right type
        /// </summary>
        /// <returns>
        /// The relationships.
        /// </returns>
        private RelationshipProxy[] GetPopulatedRelationships(LeftType leftType, IEnumerable<IIdentifiable> lefts)
        {
            IEnumerable<RelationshipProxy> relationshipsFromLeftToRight =
                _relationshipProxies.Select(entry => entry.Value).Where(proxy => proxy.LeftType == leftType);

            return relationshipsFromLeftToRight.Where(proxy => proxy.IsContextRelation || lefts.Any(resource => proxy.GetValue(resource) != null)).ToArray();
        }

        /// <summary>
        /// Registers the resources as "seen" in the tree traversal, extracts any new <see cref="RelationshipProxy" />s from it.
        /// </summary>
        /// <returns>
        /// The resources.
        /// </returns>
        /// <param name="incomingResources">
        /// Incoming resources.
        /// </param>
        /// <typeparam name="TResource">
        /// The 1st type parameter.
        /// </typeparam>
        private HashSet<TResource> ProcessResources<TResource>(IEnumerable<TResource> incomingResources)
            where TResource : class, IIdentifiable
        {
            RightType type = typeof(TResource);
            HashSet<TResource> newResources = UniqueInTree(incomingResources, type);
            RegisterProcessedResources(newResources, type);
            return newResources;
        }

        /// <summary>
        /// Parses all relationships from <paramref name="type" /> to other models in the resource resourceGraphs by constructing RelationshipProxies .
        /// </summary>
        /// <param name="type">
        /// The type to parse
        /// </param>
        private void RegisterRelationshipProxies(RightType type)
        {
            foreach (RelationshipAttribute attr in _resourceGraph.GetRelationships(type))
            {
                if (!attr.CanInclude)
                {
                    continue;
                }

                if (!_relationshipProxies.TryGetValue(attr, out _))
                {
                    RightType rightType = GetRightTypeFromRelationship(attr);
                    bool isContextRelation = false;
                    ISet<RelationshipAttribute> relationshipsToUpdate = _targetedFields.Relationships;

                    if (relationshipsToUpdate != null)
                    {
                        isContextRelation = relationshipsToUpdate.Contains(attr);
                    }

                    var proxy = new RelationshipProxy(attr, rightType, isContextRelation);
                    _relationshipProxies[attr] = proxy;
                }
            }
        }

        /// <summary>
        /// Registers the processed resources in the dictionary grouped by type
        /// </summary>
        /// <param name="resources">
        /// Resources to register
        /// </param>
        /// <param name="resourceType">
        /// Resource type.
        /// </param>
        private void RegisterProcessedResources(IEnumerable<IIdentifiable> resources, RightType resourceType)
        {
            HashSet<IIdentifiable> processedResources = GetProcessedResources(resourceType);
            processedResources.UnionWith(new HashSet<IIdentifiable>(resources));
        }

        /// <summary>
        /// Gets the processed resources for a given type, instantiates the collection if new.
        /// </summary>
        /// <returns>
        /// The processed resources.
        /// </returns>
        /// <param name="resourceType">
        /// Resource type.
        /// </param>
        private HashSet<IIdentifiable> GetProcessedResources(RightType resourceType)
        {
            if (!_processedResources.TryGetValue(resourceType, out HashSet<IIdentifiable> processedResources))
            {
                processedResources = new HashSet<IIdentifiable>();
                _processedResources[resourceType] = processedResources;
            }

            return processedResources;
        }

        /// <summary>
        /// Using the register of processed resources, determines the unique and new resources with respect to previous iterations.
        /// </summary>
        /// <returns>
        /// The in tree.
        /// </returns>
        private HashSet<TResource> UniqueInTree<TResource>(IEnumerable<TResource> resources, RightType resourceType)
            where TResource : class, IIdentifiable
        {
            IEnumerable<TResource> newResources = resources.Except(GetProcessedResources(resourceType), _comparer).Cast<TResource>();
            return new HashSet<TResource>(newResources);
        }

        /// <summary>
        /// Gets the type from relationship attribute. If the attribute is HasManyThrough, and the through type is identifiable, then the target type is the
        /// through type instead of the right type, because hooks might be implemented for the through resource.
        /// </summary>
        /// <returns>
        /// The target type for traversal
        /// </returns>
        /// <param name="attr">
        /// Relationship attribute
        /// </param>
        private RightType GetRightTypeFromRelationship(RelationshipAttribute attr)
        {
            if (attr is HasManyThroughAttribute throughAttr && TypeHelper.IsOrImplementsInterface(throughAttr.ThroughType, typeof(IIdentifiable)))
            {
                return throughAttr.ThroughType;
            }

            return attr.RightType;
        }

        private void AddToRelationshipGroup(Dictionary<RelationshipProxy, List<IIdentifiable>> target, RelationshipProxy proxy,
            IEnumerable<IIdentifiable> newResources)
        {
            if (!target.TryGetValue(proxy, out List<IIdentifiable> resources))
            {
                resources = new List<IIdentifiable>();
                target[proxy] = resources;
            }

            resources.AddRange(newResources);
        }

        /// <summary>
        /// Reflective helper method to create an instance of <see cref="ChildNode{TResource}" />;
        /// </summary>
        private IResourceNode CreateNodeInstance(RightType nodeType, RelationshipProxy[] relationshipsToNext,
            IEnumerable<IRelationshipGroup> relationshipsFromPrev)
        {
            IRelationshipsFromPreviousLayer prev = CreateRelationshipsFromInstance(nodeType, relationshipsFromPrev);
            return (IResourceNode)TypeHelper.CreateInstanceOfOpenType(typeof(ChildNode<>), nodeType, relationshipsToNext, prev);
        }

        /// <summary>
        /// Reflective helper method to create an instance of <see cref="RelationshipsFromPreviousLayer{TRight}" />;
        /// </summary>
        private IRelationshipsFromPreviousLayer CreateRelationshipsFromInstance(RightType nodeType, IEnumerable<IRelationshipGroup> relationshipsFromPrev)
        {
            List<IRelationshipGroup> relationshipsFromPrevList = relationshipsFromPrev.ToList();
            IList cast = TypeHelper.CopyToList(relationshipsFromPrevList, relationshipsFromPrevList.First().GetType());
            return (IRelationshipsFromPreviousLayer)TypeHelper.CreateInstanceOfOpenType(typeof(RelationshipsFromPreviousLayer<>), nodeType, cast);
        }

        /// <summary>
        /// Reflective helper method to create an instance of <see cref="RelationshipGroup{TRight}" />;
        /// </summary>
        private IRelationshipGroup CreateRelationshipGroupInstance(RightType thisLayerType, RelationshipProxy proxy, List<IIdentifiable> leftResources,
            List<IIdentifiable> rightResources)
        {
            object rightResourcesHashed =
                TypeHelper.CreateInstanceOfOpenType(typeof(HashSet<>), thisLayerType, TypeHelper.CopyToList(rightResources, thisLayerType));

            return (IRelationshipGroup)TypeHelper.CreateInstanceOfOpenType(typeof(RelationshipGroup<>), thisLayerType, proxy,
                new HashSet<IIdentifiable>(leftResources), rightResourcesHashed);
        }
    }
}
