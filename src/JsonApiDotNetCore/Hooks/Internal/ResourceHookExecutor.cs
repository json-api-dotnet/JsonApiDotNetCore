using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Hooks.Internal.Traversal;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using LeftType = System.Type;
using RightType = System.Type;

namespace JsonApiDotNetCore.Hooks.Internal
{
    /// <inheritdoc />
    internal sealed class ResourceHookExecutor : IResourceHookExecutor
    {
        private readonly IHookExecutorHelper _executorHelper;
        private readonly ITraversalHelper _traversalHelper;
        private readonly IEnumerable<IQueryConstraintProvider> _constraintProviders;
        private readonly ITargetedFields _targetedFields;
        private readonly IResourceGraph _resourceGraph;

        public ResourceHookExecutor(
            IHookExecutorHelper executorHelper,
            ITraversalHelper traversalHelper,
            ITargetedFields targetedFields,
            IEnumerable<IQueryConstraintProvider> constraintProviders,
            IResourceGraph resourceGraph)
        {
            _executorHelper = executorHelper;
            _traversalHelper = traversalHelper;
            _targetedFields = targetedFields;
            _constraintProviders = constraintProviders;
            _resourceGraph = resourceGraph;
        }

        /// <inheritdoc />
        public void BeforeRead<TResource>(ResourcePipeline pipeline, string stringId = null) where TResource : class, IIdentifiable
        {
            var hookContainer = _executorHelper.GetResourceHookContainer<TResource>(ResourceHook.BeforeRead);
            hookContainer?.BeforeRead(pipeline, false, stringId);
            var calledContainers = new List<LeftType> { typeof(TResource) };

            var includes = _constraintProviders
                .SelectMany(p => p.GetConstraints())
                .Select(expressionInScope => expressionInScope.Expression)
                .OfType<IncludeExpression>()
                .ToArray();

            foreach (var chain in includes.SelectMany(IncludeChainConverter.GetRelationshipChains))
            {
                RecursiveBeforeRead(chain.Fields.Cast<RelationshipAttribute>().ToList(), pipeline, calledContainers);
            }
        }

        /// <inheritdoc />
        public IEnumerable<TResource> BeforeUpdate<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline) where TResource : class, IIdentifiable
        {
            if (GetHook(ResourceHook.BeforeUpdate, resources, out var container, out var node))
            {
                var relationships = node.RelationshipsToNextLayer.Select(p => p.Attribute).ToArray();
                var dbValues = LoadDbValues(typeof(TResource), (IEnumerable<TResource>)node.UniqueResources, ResourceHook.BeforeUpdate, relationships);
                var diff = new DiffableResourceHashSet<TResource>(node.UniqueResources, dbValues, node.LeftsToNextLayer(), _targetedFields);
                IEnumerable<TResource> updated = container.BeforeUpdate(diff, pipeline);
                node.UpdateUnique(updated);
                node.Reassign(resources);
            }

            FireNestedBeforeUpdateHooks(pipeline, _traversalHelper.CreateNextLayer(node));
            return resources;
        }

        /// <inheritdoc />
        public IEnumerable<TResource> BeforeCreate<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline) where TResource : class, IIdentifiable
        {
            if (GetHook(ResourceHook.BeforeCreate, resources, out var container, out var node))
            {
                var affected = new ResourceHashSet<TResource>((HashSet<TResource>)node.UniqueResources, node.LeftsToNextLayer());
                IEnumerable<TResource> updated = container.BeforeCreate(affected, pipeline);
                node.UpdateUnique(updated);
                node.Reassign(resources);
            }
            FireNestedBeforeUpdateHooks(pipeline, _traversalHelper.CreateNextLayer(node));
            return resources;
        }

        /// <inheritdoc />
        public IEnumerable<TResource> BeforeDelete<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline) where TResource : class, IIdentifiable
        {
            if (GetHook(ResourceHook.BeforeDelete, resources, out var container, out var node))
            {
                var relationships = node.RelationshipsToNextLayer.Select(p => p.Attribute).ToArray();
                var targetResources = LoadDbValues(typeof(TResource), (IEnumerable<TResource>)node.UniqueResources, ResourceHook.BeforeDelete, relationships) ?? node.UniqueResources;
                var affected = new ResourceHashSet<TResource>(targetResources, node.LeftsToNextLayer());

                IEnumerable<TResource> updated = container.BeforeDelete(affected, pipeline);
                node.UpdateUnique(updated);
                node.Reassign(resources);
            }

            // If we're deleting an article, we're implicitly affected any owners related to it.
            // Here we're loading all relations onto the to-be-deleted article
            // if for that relation the BeforeImplicitUpdateHook is implemented,
            // and this hook is then executed
            foreach (var entry in node.LeftsToNextLayerByRelationships())
            {
                var rightType = entry.Key;
                var implicitTargets = entry.Value;
                FireForAffectedImplicits(rightType, implicitTargets, pipeline);
            }
            return resources;
        }

        /// <inheritdoc />
        public IEnumerable<TResource> OnReturn<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline) where TResource : class, IIdentifiable
        {
            if (GetHook(ResourceHook.OnReturn, resources, out var container, out var node))
            {
                IEnumerable<TResource> updated = container.OnReturn((HashSet<TResource>)node.UniqueResources, pipeline);
                ValidateHookResponse(updated);
                node.UpdateUnique(updated);
                node.Reassign(resources);
            }

            Traverse(_traversalHelper.CreateNextLayer(node), ResourceHook.OnReturn, (nextContainer, nextNode) =>
            {
                var filteredUniqueSet = CallHook(nextContainer, ResourceHook.OnReturn, new object[] { nextNode.UniqueResources, pipeline });
                nextNode.UpdateUnique(filteredUniqueSet);
                nextNode.Reassign();
            });
            return resources;
        }

        /// <inheritdoc />
        public void AfterRead<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline) where TResource : class, IIdentifiable
        {
            if (GetHook(ResourceHook.AfterRead, resources, out var container, out var node))
            {
                container.AfterRead((HashSet<TResource>)node.UniqueResources, pipeline);
            }

            Traverse(_traversalHelper.CreateNextLayer(node), ResourceHook.AfterRead, (nextContainer, nextNode) =>
            {
                CallHook(nextContainer, ResourceHook.AfterRead, new object[] { nextNode.UniqueResources, pipeline, true });
            });
        }

        /// <inheritdoc />
        public void AfterCreate<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline) where TResource : class, IIdentifiable
        {
            if (GetHook(ResourceHook.AfterCreate, resources, out var container, out var node))
            {
                container.AfterCreate((HashSet<TResource>)node.UniqueResources, pipeline);
            }

            Traverse(_traversalHelper.CreateNextLayer(node),
                ResourceHook.AfterUpdateRelationship,
                (nextContainer, nextNode) => FireAfterUpdateRelationship(nextContainer, nextNode, pipeline));
        }

        /// <inheritdoc />
        public void AfterUpdate<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline) where TResource : class, IIdentifiable
        {
            if (GetHook(ResourceHook.AfterUpdate, resources, out var container, out var node))
            {
                container.AfterUpdate((HashSet<TResource>)node.UniqueResources, pipeline);
            }

            Traverse(_traversalHelper.CreateNextLayer(node),
                ResourceHook.AfterUpdateRelationship,
                (nextContainer, nextNode) => FireAfterUpdateRelationship(nextContainer, nextNode, pipeline));
        }

        /// <inheritdoc />
        public void AfterDelete<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline, bool succeeded) where TResource : class, IIdentifiable
        {
            if (GetHook(ResourceHook.AfterDelete, resources, out var container, out var node))
            {
                container.AfterDelete((HashSet<TResource>)node.UniqueResources, pipeline, succeeded);
            }
        }

        /// <summary>
        /// For a given <see cref="ResourceHook"/> target and for a given type 
        /// <typeparamref name="TResource"/>, gets the hook container if the target
        /// hook was implemented and should be executed.
        /// <para />
        /// Along the way, creates a traversable node from the root resource set.
        /// </summary>
        /// <returns><c>true</c>, if hook was implemented, <c>false</c> otherwise.</returns>
        private bool GetHook<TResource>(ResourceHook target, IEnumerable<TResource> resources,
            out IResourceHookContainer<TResource> container,
            out RootNode<TResource> node) where TResource : class, IIdentifiable
        {
            node = _traversalHelper.CreateRootNode(resources);
            container = _executorHelper.GetResourceHookContainer<TResource>(target);
            return container != null;
        }

        /// <summary>
        /// Traverses the nodes in a <see cref="NodeLayer"/>.
        /// </summary>
        private void Traverse(NodeLayer currentLayer, ResourceHook target, Action<IResourceHookContainer, IResourceNode> action)
        {
            if (!currentLayer.AnyResources()) return;
            foreach (IResourceNode node in currentLayer)
            {
                var resourceType = node.ResourceType;
                var hookContainer = _executorHelper.GetResourceHookContainer(resourceType, target);
                if (hookContainer == null) continue;
                action(hookContainer, node);
            }

            Traverse(_traversalHelper.CreateNextLayer(currentLayer.ToList()), target, action);
        }

        /// <summary>
        /// Recursively goes through the included relationships from JsonApiContext,
        /// translates them to the corresponding hook containers and fires the 
        /// BeforeRead hook (if implemented)
        /// </summary>
        private void RecursiveBeforeRead(List<RelationshipAttribute> relationshipChain, ResourcePipeline pipeline, List<LeftType> calledContainers)
        {
            var relationship = relationshipChain.First();
            if (!calledContainers.Contains(relationship.RightType))
            {
                calledContainers.Add(relationship.RightType);
                var container = _executorHelper.GetResourceHookContainer(relationship.RightType, ResourceHook.BeforeRead);
                if (container != null)
                    CallHook(container, ResourceHook.BeforeRead, new object[] { pipeline, true, null });
            }
            relationshipChain.RemoveAt(0);
            if (relationshipChain.Any())
                RecursiveBeforeRead(relationshipChain, pipeline, calledContainers);
        }

        /// <summary>
        /// Fires the nested before hooks for resources in the current <paramref name="layer"/>
        /// </summary>
        /// <remarks>
        /// For example: consider the case when the owner of article1 (one-to-one) 
        /// is being updated from owner_old to owner_new, where owner_new is currently already 
        /// related to article2. Then, the following nested hooks need to be fired in the following order. 
        /// First the BeforeUpdateRelationship should be for owner1, then the 
        /// BeforeImplicitUpdateRelationship hook should be fired for
        /// owner2, and lastly the BeforeImplicitUpdateRelationship for article2.</remarks>
        private void FireNestedBeforeUpdateHooks(ResourcePipeline pipeline, NodeLayer layer)
        {
            foreach (IResourceNode node in layer)
            {
                var nestedHookContainer = _executorHelper.GetResourceHookContainer(node.ResourceType, ResourceHook.BeforeUpdateRelationship);
                IEnumerable uniqueResources = node.UniqueResources;
                RightType resourceType = node.ResourceType;
                Dictionary<RelationshipAttribute, IEnumerable> currentResourcesGrouped;
                Dictionary<RelationshipAttribute, IEnumerable> currentResourcesGroupedInverse;

                // fire the BeforeUpdateRelationship hook for owner_new
                if (nestedHookContainer != null)
                {
                    if (uniqueResources.Cast<IIdentifiable>().Any())
                    {
                        var relationships = node.RelationshipsToNextLayer.Select(p => p.Attribute).ToArray();
                        var dbValues = LoadDbValues(resourceType, uniqueResources, ResourceHook.BeforeUpdateRelationship, relationships);

                        // these are the resources of the current node grouped by 
                        // RelationshipAttributes that occurred in the previous layer
                        // so it looks like { HasOneAttribute:owner  =>  owner_new }.
                        // Note that in the BeforeUpdateRelationship hook of Person, 
                        // we want want inverse relationship attribute:
                        // we now have the one pointing from article -> person, ]
                        // but we require the the one that points from person -> article             
                        currentResourcesGrouped = node.RelationshipsFromPreviousLayer.GetRightResources();
                        currentResourcesGroupedInverse = ReplaceKeysWithInverseRelationships(currentResourcesGrouped);

                        var resourcesByRelationship = CreateRelationshipHelper(resourceType, currentResourcesGroupedInverse, dbValues);
                        var allowedIds = CallHook(nestedHookContainer, ResourceHook.BeforeUpdateRelationship, new object[] { GetIds(uniqueResources), resourcesByRelationship, pipeline }).Cast<string>();
                        var updated = GetAllowedResources(uniqueResources, allowedIds);
                        node.UpdateUnique(updated);
                        node.Reassign();
                    }
                }

                // Fire the BeforeImplicitUpdateRelationship hook for owner_old.
                // Note: if the pipeline is Post it means we just created article1,
                // which means we are sure that it isn't related to any other resources yet.
                if (pipeline != ResourcePipeline.Post)
                {
                    // To fire a hook for owner_old, we need to first get a reference to it.
                    // For this, we need to query the database for the  HasOneAttribute:owner 
                    // relationship of article1, which is referred to as the 
                    // left side of the HasOneAttribute:owner relationship.
                    var leftResources = node.RelationshipsFromPreviousLayer.GetLeftResources();
                    if (leftResources.Any())
                    {
                        // owner_old is loaded, which is an "implicitly affected resource"
                        FireForAffectedImplicits(resourceType, leftResources, pipeline, uniqueResources);
                    }
                }

                // Fire the BeforeImplicitUpdateRelationship hook for article2
                // For this, we need to query the database for the current owner 
                // relationship value of owner_new.
                currentResourcesGrouped = node.RelationshipsFromPreviousLayer.GetRightResources();
                if (currentResourcesGrouped.Any())
                {
                    // rightResources is grouped by relationships from previous 
                    // layer, ie { HasOneAttribute:owner  =>  owner_new }. But 
                    // to load article2 onto owner_new, we need to have the 
                    // RelationshipAttribute from owner to article, which is the
                    // inverse of HasOneAttribute:owner
                    currentResourcesGroupedInverse = ReplaceKeysWithInverseRelationships(currentResourcesGrouped);
                    // Note that currently in the JADNC implementation of hooks, 
                    // the root layer is ALWAYS homogenous, so we safely assume 
                    // that for every relationship to the previous layer, the 
                    // left type is the same.
                    LeftType leftType = currentResourcesGrouped.First().Key.LeftType;
                    FireForAffectedImplicits(leftType, currentResourcesGroupedInverse, pipeline);
                }
            }
        }

        /// <summary>
        /// replaces the keys of the <paramref name="resourcesByRelationship"/> dictionary
        /// with its inverse relationship attribute.
        /// </summary>
        /// <param name="resourcesByRelationship">Resources grouped by relationship attribute</param>
        private Dictionary<RelationshipAttribute, IEnumerable> ReplaceKeysWithInverseRelationships(Dictionary<RelationshipAttribute, IEnumerable> resourcesByRelationship)
        {
            // when Article has one Owner (HasOneAttribute:owner) is set, there is no guarantee
            // that the inverse attribute was also set (Owner has one Article: HasOneAttr:article).
            // If it isn't, JADNC currently knows nothing about this relationship pointing back, and it 
            // currently cannot fire hooks for resources resolved through inverse relationships.
            var inversableRelationshipAttributes = resourcesByRelationship.Where(kvp => kvp.Key.InverseNavigationProperty != null);
            return inversableRelationshipAttributes.ToDictionary(kvp => _resourceGraph.GetInverseRelationship(kvp.Key), kvp => kvp.Value);
        }

        /// <summary>
        /// Given a source of resources, gets the implicitly affected resources 
        /// from the database and calls the BeforeImplicitUpdateRelationship hook.
        /// </summary>
        private void FireForAffectedImplicits(Type resourceTypeToInclude, Dictionary<RelationshipAttribute, IEnumerable> implicitsTarget, ResourcePipeline pipeline, IEnumerable existingImplicitResources = null)
        {
            var container = _executorHelper.GetResourceHookContainer(resourceTypeToInclude, ResourceHook.BeforeImplicitUpdateRelationship);
            if (container == null) return;
            var implicitAffected = _executorHelper.LoadImplicitlyAffected(implicitsTarget, existingImplicitResources);
            if (!implicitAffected.Any()) return;
            var inverse = implicitAffected.ToDictionary(kvp => _resourceGraph.GetInverseRelationship(kvp.Key), kvp => kvp.Value);
            var resourcesByRelationship = CreateRelationshipHelper(resourceTypeToInclude, inverse);
            CallHook(container, ResourceHook.BeforeImplicitUpdateRelationship, new object[] { resourcesByRelationship, pipeline});
        }

        /// <summary>
        /// checks that the collection does not contain more than one item when
        /// relevant (eg AfterRead from GetSingle pipeline).
        /// </summary>
        /// <param name="returnedList"> The collection returned from the hook</param>
        /// <param name="pipeline">The pipeline from which the hook was fired</param>
        private void ValidateHookResponse<T>(IEnumerable<T> returnedList, ResourcePipeline pipeline = 0)
        {
            if (pipeline == ResourcePipeline.GetSingle && returnedList.Count() > 1)
            {
                throw new ApplicationException("The returned collection from this hook may contain at most one item in the case of the" +
                    pipeline.ToString("G") + "pipeline");
            }
        }

        /// <summary>
        /// A helper method to call a hook on <paramref name="container"/> reflectively.
        /// </summary>
        private IEnumerable CallHook(IResourceHookContainer container, ResourceHook hook, object[] arguments)
        {
            var method = container.GetType().GetMethod(hook.ToString("G"));
            // note that some of the hooks return "void". When these hooks, the 
            // are called reflectively with Invoke like here, the return value
            // is just null, so we don't have to worry about casting issues here.
            return (IEnumerable)ThrowJsonApiExceptionOnError(() => method.Invoke(container, arguments));
        }

        /// <summary>
        /// If the <see cref="CallHook"/> method, unwrap and throw the actual exception.
        /// </summary>
        private object ThrowJsonApiExceptionOnError(Func<object> action)
        {
            try
            {
                return action();
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        /// <summary>
        /// Helper method to instantiate AffectedRelationships for a given <paramref name="resourceType"/>
        /// If <paramref name="dbValues"/> are included, the values of the entries in <paramref name="prevLayerRelationships"/> need to be replaced with these values.
        /// </summary>
        /// <returns>The relationship helper.</returns>
        private IRelationshipsDictionary CreateRelationshipHelper(RightType resourceType, Dictionary<RelationshipAttribute, IEnumerable> prevLayerRelationships, IEnumerable dbValues = null)
        {
            if (dbValues != null) prevLayerRelationships = ReplaceWithDbValues(prevLayerRelationships, dbValues.Cast<IIdentifiable>());
            return (IRelationshipsDictionary)TypeHelper.CreateInstanceOfOpenType(typeof(RelationshipsDictionary<>), resourceType, true, prevLayerRelationships);
        }

        /// <summary>
        /// Replaces the resources in the values of the prevLayerRelationships dictionary 
        /// with the corresponding resources loaded from the db.
        /// </summary>
        private Dictionary<RelationshipAttribute, IEnumerable> ReplaceWithDbValues(Dictionary<RelationshipAttribute, IEnumerable> prevLayerRelationships, IEnumerable<IIdentifiable> dbValues)
        {
            foreach (var key in prevLayerRelationships.Keys.ToList())
            {
                var replaced = TypeHelper.CopyToList(prevLayerRelationships[key].Cast<IIdentifiable>().Select(resource => dbValues.Single(dbResource => dbResource.StringId == resource.StringId)), key.LeftType);
                prevLayerRelationships[key] = TypeHelper.CreateHashSetFor(key.LeftType, replaced);
            }
            return prevLayerRelationships;
        }

        /// <summary>
        /// Filter the source set by removing the resources with ID that are not 
        /// in <paramref name="allowedIds"/>.
        /// </summary>
        private HashSet<IIdentifiable> GetAllowedResources(IEnumerable source, IEnumerable<string> allowedIds)
        {
            return new HashSet<IIdentifiable>(source.Cast<IIdentifiable>().Where(ue => allowedIds.Contains(ue.StringId)));
        }

        /// <summary>
        /// given the set of <paramref name="uniqueResources"/>, it will load all the 
        /// values from the database of these resources.
        /// </summary>
        /// <returns>The db values.</returns>
        /// <param name="resourceType">type of the resources to be loaded</param>
        /// <param name="uniqueResources">The set of resources to load the db values for</param>
        /// <param name="targetHook">The hook in which the db values will be displayed.</param>
        /// <param name="relationshipsToNextLayer">Relationships from <paramref name="resourceType"/> to the next layer: 
        /// this indicates which relationships will be included on <paramref name="uniqueResources"/>.</param>
        private IEnumerable LoadDbValues(Type resourceType, IEnumerable uniqueResources, ResourceHook targetHook, RelationshipAttribute[] relationshipsToNextLayer)
        {
            // We only need to load database values if the target hook of this hook execution
            // cycle is compatible with displaying database values and has this option enabled.
            if (!_executorHelper.ShouldLoadDbValues(resourceType, targetHook)) return null;
            return _executorHelper.LoadDbValues(resourceType, uniqueResources, targetHook, relationshipsToNextLayer);
        }

        /// <summary>
        /// Fires the AfterUpdateRelationship hook
        /// </summary>
        private void FireAfterUpdateRelationship(IResourceHookContainer container, IResourceNode node, ResourcePipeline pipeline)
        {

            Dictionary<RelationshipAttribute, IEnumerable> currentResourcesGrouped = node.RelationshipsFromPreviousLayer.GetRightResources();
            // the relationships attributes in currentResourcesGrouped will be pointing from a 
            // resource in the previous layer to a resource in the current (nested) layer.
            // For the nested hook we need to replace these attributes with their inverse.
            // See the FireNestedBeforeUpdateHooks method for a more detailed example.
            var resourcesByRelationship = CreateRelationshipHelper(node.ResourceType, ReplaceKeysWithInverseRelationships(currentResourcesGrouped));
            CallHook(container, ResourceHook.AfterUpdateRelationship, new object[] { resourcesByRelationship, pipeline });
        }

        /// <summary>
        /// Returns a list of StringIds from a list of IIdentifiable resources (<paramref name="resources"/>).
        /// </summary>
        /// <returns>The ids.</returns>
        /// <param name="resources">IIdentifiable resources.</param>
        private HashSet<string> GetIds(IEnumerable resources)
        {
            return new HashSet<string>(resources.Cast<IIdentifiable>().Select(e => e.StringId));
        }
    }
}
