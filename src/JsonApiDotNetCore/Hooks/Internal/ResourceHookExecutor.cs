using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Hooks.Internal.Traversal;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using LeftType = System.Type;
using RightType = System.Type;

// ReSharper disable PossibleMultipleEnumeration

namespace JsonApiDotNetCore.Hooks.Internal
{
    /// <inheritdoc />
    internal sealed class ResourceHookExecutor : IResourceHookExecutor
    {
        private static readonly IncludeChainConverter IncludeChainConverter = new IncludeChainConverter();
        private static readonly HooksObjectFactory ObjectFactory = new HooksObjectFactory();
        private static readonly HooksCollectionConverter CollectionConverter = new HooksCollectionConverter();

        private readonly IHookContainerProvider _containerProvider;
        private readonly INodeNavigator _nodeNavigator;
        private readonly IEnumerable<IQueryConstraintProvider> _constraintProviders;
        private readonly ITargetedFields _targetedFields;
        private readonly IResourceGraph _resourceGraph;

        public ResourceHookExecutor(IHookContainerProvider containerProvider, INodeNavigator nodeNavigator, ITargetedFields targetedFields,
            IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceGraph resourceGraph)
        {
            _containerProvider = containerProvider;
            _nodeNavigator = nodeNavigator;
            _targetedFields = targetedFields;
            _constraintProviders = constraintProviders;
            _resourceGraph = resourceGraph;
        }

        /// <inheritdoc />
        public void BeforeRead<TResource>(ResourcePipeline pipeline, string stringId = null)
            where TResource : class, IIdentifiable
        {
            IResourceHookContainer<TResource> hookContainer = _containerProvider.GetResourceHookContainer<TResource>(ResourceHook.BeforeRead);
            hookContainer?.BeforeRead(pipeline, false, stringId);
            List<Type> calledContainers = typeof(TResource).AsList();

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            IncludeExpression[] includes = _constraintProviders
                .SelectMany(provider => provider.GetConstraints())
                .Select(expressionInScope => expressionInScope.Expression)
                .OfType<IncludeExpression>()
                .ToArray();

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            foreach (ResourceFieldChainExpression chain in includes.SelectMany(IncludeChainConverter.GetRelationshipChains))
            {
                RecursiveBeforeRead(chain.Fields.Cast<RelationshipAttribute>().ToList(), pipeline, calledContainers);
            }
        }

        /// <inheritdoc />
        public IEnumerable<TResource> BeforeUpdate<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable
        {
            GetHookResult<TResource> result = GetHook(ResourceHook.BeforeUpdate, resources);

            if (result.Succeeded)
            {
                RelationshipAttribute[] relationships = result.Node.RelationshipsToNextLayer.Select(proxy => proxy.Attribute).ToArray();

                IEnumerable dbValues = LoadDbValues(typeof(TResource), (IEnumerable<TResource>)result.Node.UniqueResources, ResourceHook.BeforeUpdate,
                    relationships);

                var diff = new DiffableResourceHashSet<TResource>(result.Node.UniqueResources, dbValues, result.Node.LeftsToNextLayer(), _targetedFields);
                IEnumerable<TResource> updated = result.Container.BeforeUpdate(diff, pipeline);
                result.Node.UpdateUnique(updated);
                result.Node.Reassign(resources);
            }

            FireNestedBeforeUpdateHooks(pipeline, _nodeNavigator.CreateNextLayer(result.Node));
            return resources;
        }

        /// <inheritdoc />
        public IEnumerable<TResource> BeforeCreate<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable
        {
            GetHookResult<TResource> result = GetHook(ResourceHook.BeforeCreate, resources);

            if (result.Succeeded)
            {
                var affected = new ResourceHashSet<TResource>((HashSet<TResource>)result.Node.UniqueResources, result.Node.LeftsToNextLayer());
                IEnumerable<TResource> updated = result.Container.BeforeCreate(affected, pipeline);
                result.Node.UpdateUnique(updated);
                result.Node.Reassign(resources);
            }

            FireNestedBeforeUpdateHooks(pipeline, _nodeNavigator.CreateNextLayer(result.Node));
            return resources;
        }

        /// <inheritdoc />
        public IEnumerable<TResource> BeforeDelete<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable
        {
            GetHookResult<TResource> result = GetHook(ResourceHook.BeforeDelete, resources);

            if (result.Succeeded)
            {
                RelationshipAttribute[] relationships = result.Node.RelationshipsToNextLayer.Select(proxy => proxy.Attribute).ToArray();

                IEnumerable targetResources =
                    LoadDbValues(typeof(TResource), (IEnumerable<TResource>)result.Node.UniqueResources, ResourceHook.BeforeDelete, relationships) ??
                    result.Node.UniqueResources;

                var affected = new ResourceHashSet<TResource>(targetResources, result.Node.LeftsToNextLayer());

                IEnumerable<TResource> updated = result.Container.BeforeDelete(affected, pipeline);
                result.Node.UpdateUnique(updated);
                result.Node.Reassign(resources);
            }

            // If we're deleting an article, we're implicitly affected any owners related to it.
            // Here we're loading all relations onto the to-be-deleted article
            // if for that relation the BeforeImplicitUpdateHook is implemented,
            // and this hook is then executed
            foreach (KeyValuePair<Type, Dictionary<RelationshipAttribute, IEnumerable>> entry in result.Node.LeftsToNextLayerByRelationships())
            {
                Type rightType = entry.Key;
                Dictionary<RelationshipAttribute, IEnumerable> implicitTargets = entry.Value;
                FireForAffectedImplicits(rightType, implicitTargets, pipeline);
            }

            return resources;
        }

        /// <inheritdoc />
        public IEnumerable<TResource> OnReturn<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable
        {
            GetHookResult<TResource> result = GetHook(ResourceHook.OnReturn, resources);

            if (result.Succeeded)
            {
                IEnumerable<TResource> updated = result.Container.OnReturn((HashSet<TResource>)result.Node.UniqueResources, pipeline);
                ValidateHookResponse(updated);
                result.Node.UpdateUnique(updated);
                result.Node.Reassign(resources);
            }

            TraverseNodesInLayer(_nodeNavigator.CreateNextLayer(result.Node), ResourceHook.OnReturn, (nextContainer, nextNode) =>
            {
                IEnumerable filteredUniqueSet = CallHook(nextContainer, ResourceHook.OnReturn, ArrayFactory.Create<object>(nextNode.UniqueResources, pipeline));
                nextNode.UpdateUnique(filteredUniqueSet);
                nextNode.Reassign();
            });

            return resources;
        }

        /// <inheritdoc />
        public void AfterRead<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable
        {
            GetHookResult<TResource> result = GetHook(ResourceHook.AfterRead, resources);

            if (result.Succeeded)
            {
                result.Container.AfterRead((HashSet<TResource>)result.Node.UniqueResources, pipeline);
            }

            TraverseNodesInLayer(_nodeNavigator.CreateNextLayer(result.Node), ResourceHook.AfterRead, (nextContainer, nextNode) =>
            {
                CallHook(nextContainer, ResourceHook.AfterRead, ArrayFactory.Create<object>(nextNode.UniqueResources, pipeline, true));
            });
        }

        /// <inheritdoc />
        public void AfterCreate<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable
        {
            GetHookResult<TResource> result = GetHook(ResourceHook.AfterCreate, resources);

            if (result.Succeeded)
            {
                result.Container.AfterCreate((HashSet<TResource>)result.Node.UniqueResources, pipeline);
            }

            TraverseNodesInLayer(_nodeNavigator.CreateNextLayer(result.Node), ResourceHook.AfterUpdateRelationship,
                (nextContainer, nextNode) => FireAfterUpdateRelationship(nextContainer, nextNode, pipeline));
        }

        /// <inheritdoc />
        public void AfterUpdate<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable
        {
            GetHookResult<TResource> result = GetHook(ResourceHook.AfterUpdate, resources);

            if (result.Succeeded)
            {
                result.Container.AfterUpdate((HashSet<TResource>)result.Node.UniqueResources, pipeline);
            }

            TraverseNodesInLayer(_nodeNavigator.CreateNextLayer(result.Node), ResourceHook.AfterUpdateRelationship,
                (nextContainer, nextNode) => FireAfterUpdateRelationship(nextContainer, nextNode, pipeline));
        }

        /// <inheritdoc />
        public void AfterDelete<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline, bool succeeded)
            where TResource : class, IIdentifiable
        {
            GetHookResult<TResource> result = GetHook(ResourceHook.AfterDelete, resources);

            if (result.Succeeded)
            {
                result.Container.AfterDelete((HashSet<TResource>)result.Node.UniqueResources, pipeline, succeeded);
            }
        }

        /// <summary>
        /// For a given <see cref="ResourceHook" /> target and for a given type <typeparamref name="TResource" />, gets the hook container if the target hook was
        /// implemented and should be executed.
        /// <para />
        /// Along the way, creates a traversable node from the root resource set.
        /// </summary>
        /// <returns>
        /// <c>true</c>, if hook was implemented, <c>false</c> otherwise.
        /// </returns>
        private GetHookResult<TResource> GetHook<TResource>(ResourceHook target, IEnumerable<TResource> resources)
            where TResource : class, IIdentifiable
        {
            RootNode<TResource> node = _nodeNavigator.CreateRootNode(resources);
            IResourceHookContainer<TResource> container = _containerProvider.GetResourceHookContainer<TResource>(target);

            return new GetHookResult<TResource>(container, node);
        }

        private void TraverseNodesInLayer(IEnumerable<IResourceNode> currentLayer, ResourceHook target, Action<IResourceHookContainer, IResourceNode> action)
        {
            IEnumerable<IResourceNode> nextLayer = currentLayer;

            while (true)
            {
                if (!HasResources(nextLayer))
                {
                    return;
                }

                TraverseNextLayer(nextLayer, action, target);

                nextLayer = _nodeNavigator.CreateNextLayer(nextLayer.ToList());
            }
        }

        private static bool HasResources(IEnumerable<IResourceNode> layer)
        {
            return layer.Any(node => node.UniqueResources.Cast<IIdentifiable>().Any());
        }

        private void TraverseNextLayer(IEnumerable<IResourceNode> nextLayer, Action<IResourceHookContainer, IResourceNode> action, ResourceHook target)
        {
            foreach (IResourceNode node in nextLayer)
            {
                IResourceHookContainer hookContainer = _containerProvider.GetResourceHookContainer(node.ResourceType, target);

                if (hookContainer != null)
                {
                    action(hookContainer, node);
                }
            }
        }

        /// <summary>
        /// Recursively goes through the included relationships from JsonApiContext, translates them to the corresponding hook containers and fires the
        /// BeforeRead hook (if implemented)
        /// </summary>
        private void RecursiveBeforeRead(List<RelationshipAttribute> relationshipChain, ResourcePipeline pipeline, List<LeftType> calledContainers)
        {
            while (true)
            {
                RelationshipAttribute relationship = relationshipChain.First();

                if (!calledContainers.Contains(relationship.RightType))
                {
                    calledContainers.Add(relationship.RightType);
                    IResourceHookContainer container = _containerProvider.GetResourceHookContainer(relationship.RightType, ResourceHook.BeforeRead);

                    if (container != null)
                    {
                        CallHook(container, ResourceHook.BeforeRead, new object[]
                        {
                            pipeline,
                            true,
                            null
                        });
                    }
                }

                relationshipChain.RemoveAt(0);

                if (!relationshipChain.Any())
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Fires the nested before hooks for resources in the current <paramref name="layer" />
        /// </summary>
        /// <remarks>
        /// For example: consider the case when the owner of article1 (one-to-one) is being updated from owner_old to owner_new, where owner_new is currently
        /// already related to article2. Then, the following nested hooks need to be fired in the following order. First the BeforeUpdateRelationship should be
        /// for owner1, then the BeforeImplicitUpdateRelationship hook should be fired for owner2, and lastly the BeforeImplicitUpdateRelationship for article2.
        /// </remarks>
        private void FireNestedBeforeUpdateHooks(ResourcePipeline pipeline, IEnumerable<IResourceNode> layer)
        {
            foreach (IResourceNode node in layer)
            {
                IResourceHookContainer nestedHookContainer =
                    _containerProvider.GetResourceHookContainer(node.ResourceType, ResourceHook.BeforeUpdateRelationship);

                IEnumerable uniqueResources = node.UniqueResources;
                RightType resourceType = node.ResourceType;
                IDictionary<RelationshipAttribute, IEnumerable> currentResourcesGrouped;
                IDictionary<RelationshipAttribute, IEnumerable> currentResourcesGroupedInverse;

                // fire the BeforeUpdateRelationship hook for owner_new
                if (nestedHookContainer != null)
                {
                    if (uniqueResources.Cast<IIdentifiable>().Any())
                    {
                        RelationshipAttribute[] relationships = node.RelationshipsToNextLayer.Select(proxy => proxy.Attribute).ToArray();
                        IEnumerable dbValues = LoadDbValues(resourceType, uniqueResources, ResourceHook.BeforeUpdateRelationship, relationships);

                        // these are the resources of the current node grouped by
                        // RelationshipAttributes that occurred in the previous layer
                        // so it looks like { HasOneAttribute:owner  =>  owner_new }.
                        // Note that in the BeforeUpdateRelationship hook of Person,
                        // we want want inverse relationship attribute:
                        // we now have the one pointing from article -> person, ]
                        // but we require the the one that points from person -> article
                        currentResourcesGrouped = node.RelationshipsFromPreviousLayer.GetRightResources();
                        currentResourcesGroupedInverse = ReplaceKeysWithInverseRelationships(currentResourcesGrouped);

                        IRelationshipsDictionary resourcesByRelationship = CreateRelationshipHelper(resourceType, currentResourcesGroupedInverse, dbValues);

                        IEnumerable<string> allowedIds = CallHook(nestedHookContainer, ResourceHook.BeforeUpdateRelationship,
                            ArrayFactory.Create<object>(GetIds(uniqueResources), resourcesByRelationship, pipeline)).Cast<string>();

                        ISet<IIdentifiable> updated = GetAllowedResources(uniqueResources, allowedIds);
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
                    IDictionary<RelationshipAttribute, IEnumerable> leftResources = node.RelationshipsFromPreviousLayer.GetLeftResources();

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
                    // Note that currently in the JsonApiDotNetCore implementation of hooks,
                    // the root layer is ALWAYS homogenous, so we safely assume
                    // that for every relationship to the previous layer, the
                    // left type is the same.
                    LeftType leftType = currentResourcesGrouped.First().Key.LeftType;
                    FireForAffectedImplicits(leftType, currentResourcesGroupedInverse, pipeline);
                }
            }
        }

        /// <summary>
        /// replaces the keys of the <paramref name="resourcesByRelationship" /> dictionary with its inverse relationship attribute.
        /// </summary>
        /// <param name="resourcesByRelationship">
        /// Resources grouped by relationship attribute
        /// </param>
        private IDictionary<RelationshipAttribute, IEnumerable> ReplaceKeysWithInverseRelationships(
            IDictionary<RelationshipAttribute, IEnumerable> resourcesByRelationship)
        {
            // when Article has one Owner (HasOneAttribute:owner) is set, there is no guarantee
            // that the inverse attribute was also set (Owner has one Article: HasOneAttr:article).
            // If it isn't, JsonApiDotNetCore currently knows nothing about this relationship pointing back, and it
            // currently cannot fire hooks for resources resolved through inverse relationships.
            IEnumerable<KeyValuePair<RelationshipAttribute, IEnumerable>> inversableRelationshipAttributes =
                resourcesByRelationship.Where(pair => pair.Key.InverseNavigationProperty != null);

            return inversableRelationshipAttributes.ToDictionary(pair => _resourceGraph.GetInverseRelationship(pair.Key), pair => pair.Value);
        }

        /// <summary>
        /// Given a source of resources, gets the implicitly affected resources from the database and calls the BeforeImplicitUpdateRelationship hook.
        /// </summary>
        private void FireForAffectedImplicits(Type resourceTypeToInclude, IDictionary<RelationshipAttribute, IEnumerable> implicitsTarget,
            ResourcePipeline pipeline, IEnumerable existingImplicitResources = null)
        {
            IResourceHookContainer container =
                _containerProvider.GetResourceHookContainer(resourceTypeToInclude, ResourceHook.BeforeImplicitUpdateRelationship);

            if (container == null)
            {
                return;
            }

            IDictionary<RelationshipAttribute, IEnumerable> implicitAffected =
                _containerProvider.LoadImplicitlyAffected(implicitsTarget, existingImplicitResources);

            if (!implicitAffected.Any())
            {
                return;
            }

            Dictionary<RelationshipAttribute, IEnumerable> inverse =
                implicitAffected.ToDictionary(pair => _resourceGraph.GetInverseRelationship(pair.Key), pair => pair.Value);

            IRelationshipsDictionary resourcesByRelationship = CreateRelationshipHelper(resourceTypeToInclude, inverse);
            CallHook(container, ResourceHook.BeforeImplicitUpdateRelationship, ArrayFactory.Create<object>(resourcesByRelationship, pipeline));
        }

        /// <summary>
        /// checks that the collection does not contain more than one item when relevant (eg AfterRead from GetSingle pipeline).
        /// </summary>
        /// <param name="returnedList">
        /// The collection returned from the hook
        /// </param>
        /// <param name="pipeline">
        /// The pipeline from which the hook was fired
        /// </param>
        [AssertionMethod]
        private void ValidateHookResponse<T>(IEnumerable<T> returnedList, ResourcePipeline pipeline = 0)
        {
            if (pipeline == ResourcePipeline.GetSingle && returnedList.Count() > 1)
            {
                throw new InvalidOperationException("The returned collection from this hook may contain at most one item in the case of the " +
                    pipeline.ToString("G") + " pipeline");
            }
        }

        /// <summary>
        /// A helper method to call a hook on <paramref name="container" /> reflectively.
        /// </summary>
        private IEnumerable CallHook(IResourceHookContainer container, ResourceHook hook, object[] arguments)
        {
            MethodInfo method = container.GetType().GetMethod(hook.ToString("G"));
            // note that some of the hooks return "void". When these hooks, the
            // are called reflectively with Invoke like here, the return value
            // is just null, so we don't have to worry about casting issues here.
            return (IEnumerable)ThrowJsonApiExceptionOnError(() => method?.Invoke(container, arguments));
        }

        /// <summary>
        /// If the <see cref="CallHook" /> method, unwrap and throw the actual exception.
        /// </summary>
        private object ThrowJsonApiExceptionOnError(Func<object> action)
        {
            try
            {
                return action();
            }
            catch (TargetInvocationException tie) when (tie.InnerException != null)
            {
                throw tie.InnerException;
            }
        }

        /// <summary>
        /// Helper method to instantiate AffectedRelationships for a given <paramref name="resourceType" /> If <paramref name="dbValues" /> are included, the
        /// values of the entries in <paramref name="prevLayerRelationships" /> need to be replaced with these values.
        /// </summary>
        /// <returns>
        /// The relationship helper.
        /// </returns>
        private IRelationshipsDictionary CreateRelationshipHelper(RightType resourceType,
            IDictionary<RelationshipAttribute, IEnumerable> prevLayerRelationships, IEnumerable dbValues = null)
        {
            IDictionary<RelationshipAttribute, IEnumerable> prevLayerRelationshipsWithDbValues = prevLayerRelationships;

            if (dbValues != null)
            {
                prevLayerRelationshipsWithDbValues = ReplaceWithDbValues(prevLayerRelationshipsWithDbValues, dbValues.Cast<IIdentifiable>());
            }

            return (IRelationshipsDictionary)ObjectFactory.CreateInstanceOfInternalOpenType(typeof(RelationshipsDictionary<>), resourceType,
                prevLayerRelationshipsWithDbValues);
        }

        /// <summary>
        /// Replaces the resources in the values of the prevLayerRelationships dictionary with the corresponding resources loaded from the db.
        /// </summary>
        private IDictionary<RelationshipAttribute, IEnumerable> ReplaceWithDbValues(IDictionary<RelationshipAttribute, IEnumerable> prevLayerRelationships,
            IEnumerable<IIdentifiable> dbValues)
        {
            foreach (RelationshipAttribute key in prevLayerRelationships.Keys.ToList())
            {
                IEnumerable<IIdentifiable> source = prevLayerRelationships[key].Cast<IIdentifiable>().Select(resource =>
                    dbValues.Single(dbResource => dbResource.StringId == resource.StringId));

                prevLayerRelationships[key] = CollectionConverter.CopyToHashSet(source, key.LeftType);
            }

            return prevLayerRelationships;
        }

        /// <summary>
        /// Filter the source set by removing the resources with ID that are not in <paramref name="allowedIds" />.
        /// </summary>
        private ISet<IIdentifiable> GetAllowedResources(IEnumerable source, IEnumerable<string> allowedIds)
        {
            return new HashSet<IIdentifiable>(source.Cast<IIdentifiable>().Where(ue => allowedIds.Contains(ue.StringId)));
        }

        /// <summary>
        /// given the set of <paramref name="uniqueResources" />, it will load all the values from the database of these resources.
        /// </summary>
        /// <returns>
        /// The db values.
        /// </returns>
        /// <param name="resourceType">
        /// type of the resources to be loaded
        /// </param>
        /// <param name="uniqueResources">
        /// The set of resources to load the db values for
        /// </param>
        /// <param name="targetHook">
        /// The hook in which the db values will be displayed.
        /// </param>
        /// <param name="relationshipsToNextLayer">
        /// Relationships from <paramref name="resourceType" /> to the next layer: this indicates which relationships will be included on
        /// <paramref name="uniqueResources" />.
        /// </param>
        private IEnumerable LoadDbValues(Type resourceType, IEnumerable uniqueResources, ResourceHook targetHook,
            RelationshipAttribute[] relationshipsToNextLayer)
        {
            // We only need to load database values if the target hook of this hook execution
            // cycle is compatible with displaying database values and has this option enabled.
            if (!_containerProvider.ShouldLoadDbValues(resourceType, targetHook))
            {
                return null;
            }

            return _containerProvider.LoadDbValues(resourceType, uniqueResources, relationshipsToNextLayer);
        }

        /// <summary>
        /// Fires the AfterUpdateRelationship hook
        /// </summary>
        private void FireAfterUpdateRelationship(IResourceHookContainer container, IResourceNode node, ResourcePipeline pipeline)
        {
            IDictionary<RelationshipAttribute, IEnumerable> currentResourcesGrouped = node.RelationshipsFromPreviousLayer.GetRightResources();

            // the relationships attributes in currentResourcesGrouped will be pointing from a
            // resource in the previous layer to a resource in the current (nested) layer.
            // For the nested hook we need to replace these attributes with their inverse.
            // See the FireNestedBeforeUpdateHooks method for a more detailed example.
            IRelationshipsDictionary resourcesByRelationship =
                CreateRelationshipHelper(node.ResourceType, ReplaceKeysWithInverseRelationships(currentResourcesGrouped));

            CallHook(container, ResourceHook.AfterUpdateRelationship, ArrayFactory.Create<object>(resourcesByRelationship, pipeline));
        }

        /// <summary>
        /// Returns a list of StringIds from a list of IIdentifiable resources (<paramref name="resources" />).
        /// </summary>
        /// <returns>The ids.</returns>
        /// <param name="resources">
        /// IIdentifiable resources.
        /// </param>
        private ISet<string> GetIds(IEnumerable resources)
        {
            return new HashSet<string>(resources.Cast<IIdentifiable>().Select(resource => resource.StringId));
        }

        private sealed class GetHookResult<TResource>
            where TResource : class, IIdentifiable
        {
            public IResourceHookContainer<TResource> Container { get; }
            public RootNode<TResource> Node { get; }

            public bool Succeeded => Container != null;

            public GetHookResult(IResourceHookContainer<TResource> container, RootNode<TResource> node)
            {
                Container = container;
                Node = node;
            }
        }
    }
}
