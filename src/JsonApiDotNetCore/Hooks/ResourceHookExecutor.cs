using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using PrincipalType = System.Type;
using DependentType = System.Type;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Query;

namespace JsonApiDotNetCore.Hooks
{
    /// <inheritdoc/>
    internal class ResourceHookExecutor : IResourceHookExecutor
    {
        internal readonly IHookExecutorHelper _executorHelper;
        private readonly ITraversalHelper _traversalHelper;
        private readonly IIncludeService _includeService;
        private readonly ITargetedFields _targetedFields;
        private readonly IResourceGraph _resourceGraph;
        public ResourceHookExecutor(
            IHookExecutorHelper executorHelper,
            ITraversalHelper traversalHelper,
            ITargetedFields targetedFields,
            IIncludeService includedRelationships,
            IResourceGraph resourceGraph)
        {
            _executorHelper = executorHelper;
            _traversalHelper = traversalHelper;
            _targetedFields = targetedFields;
            _includeService = includedRelationships;
            _resourceGraph = resourceGraph;
        }

        /// <inheritdoc/>
        public virtual void BeforeRead<TResource>(ResourcePipeline pipeline, string stringId = null) where TResource : class, IIdentifiable
        {
            var hookContainer = _executorHelper.GetResourceHookContainer<TResource>(ResourceHook.BeforeRead);
            hookContainer?.BeforeRead(pipeline, false, stringId);
            var calledContainers = new List<PrincipalType>() { typeof(TResource) };
            foreach (var chain in _includeService.Get())
                RecursiveBeforeRead(chain, pipeline, calledContainers);
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TResource> BeforeUpdate<TResource>(IEnumerable<TResource> entities, ResourcePipeline pipeline) where TResource : class, IIdentifiable
        {
            if (GetHook(ResourceHook.BeforeUpdate, entities, out var container, out var node))
            {
                var relationships = node.RelationshipsToNextLayer.Select(p => p.Attribute).ToArray();
                var dbValues = LoadDbValues(typeof(TResource), (IEnumerable<TResource>)node.UniqueEntities, ResourceHook.BeforeUpdate, relationships);
                var diff = new DiffableEntityHashSet<TResource>(node.UniqueEntities, dbValues, node.PrincipalsToNextLayer(), _targetedFields);
                IEnumerable<TResource> updated = container.BeforeUpdate(diff, pipeline);
                node.UpdateUnique(updated);
                node.Reassign(entities);
            }

            FireNestedBeforeUpdateHooks(pipeline, _traversalHelper.CreateNextLayer(node));
            return entities;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TResource> BeforeCreate<TResource>(IEnumerable<TResource> entities, ResourcePipeline pipeline) where TResource : class, IIdentifiable
        {
            if (GetHook(ResourceHook.BeforeCreate, entities, out var container, out var node))
            {
                var affected = new EntityHashSet<TResource>((HashSet<TResource>)node.UniqueEntities, node.PrincipalsToNextLayer());
                IEnumerable<TResource> updated = container.BeforeCreate(affected, pipeline);
                node.UpdateUnique(updated);
                node.Reassign(entities);
            }
            FireNestedBeforeUpdateHooks(pipeline, _traversalHelper.CreateNextLayer(node));
            return entities;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TResource> BeforeDelete<TResource>(IEnumerable<TResource> entities, ResourcePipeline pipeline) where TResource : class, IIdentifiable
        {
            if (GetHook(ResourceHook.BeforeDelete, entities, out var container, out var node))
            {
                var relationships = node.RelationshipsToNextLayer.Select(p => p.Attribute).ToArray();
                var targetEntities = LoadDbValues(typeof(TResource), (IEnumerable<TResource>)node.UniqueEntities, ResourceHook.BeforeDelete, relationships) ?? node.UniqueEntities;
                var affected = new EntityHashSet<TResource>(targetEntities, node.PrincipalsToNextLayer());

                IEnumerable<TResource> updated = container.BeforeDelete(affected, pipeline);
                node.UpdateUnique(updated);
                node.Reassign(entities);
            }

            /// If we're deleting an article, we're implicitly affected any owners related to it.
            /// Here we're loading all relations onto the to-be-deleted article
            /// if for that relation the BeforeImplicitUpdateHook is implemented,
            /// and this hook is then executed
            foreach (var entry in node.PrincipalsToNextLayerByRelationships())
            {
                var dependentType = entry.Key;
                var implicitTargets = entry.Value;
                FireForAffectedImplicits(dependentType, implicitTargets, pipeline);
            }
            return entities;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TResource> OnReturn<TResource>(IEnumerable<TResource> entities, ResourcePipeline pipeline) where TResource : class, IIdentifiable
        {
            if (GetHook(ResourceHook.OnReturn, entities, out var container, out var node) && pipeline != ResourcePipeline.GetRelationship)
            {
                IEnumerable<TResource> updated = container.OnReturn((HashSet<TResource>)node.UniqueEntities, pipeline);
                ValidateHookResponse(updated);
                node.UpdateUnique(updated);
                node.Reassign(entities);
            }

            Traverse(_traversalHelper.CreateNextLayer(node), ResourceHook.OnReturn, (nextContainer, nextNode) =>
            {
                var filteredUniqueSet = CallHook(nextContainer, ResourceHook.OnReturn, new object[] { nextNode.UniqueEntities, pipeline });
                nextNode.UpdateUnique(filteredUniqueSet);
                nextNode.Reassign();
            });
            return entities;
        }

        /// <inheritdoc/>
        public virtual void AfterRead<TResource>(IEnumerable<TResource> entities, ResourcePipeline pipeline) where TResource : class, IIdentifiable
        {
            if (GetHook(ResourceHook.AfterRead, entities, out var container, out var node))
            {
                container.AfterRead((HashSet<TResource>)node.UniqueEntities, pipeline);
            }

            Traverse(_traversalHelper.CreateNextLayer(node), ResourceHook.AfterRead, (nextContainer, nextNode) =>
            {
                CallHook(nextContainer, ResourceHook.AfterRead, new object[] { nextNode.UniqueEntities, pipeline, true });
            });
        }

        /// <inheritdoc/>
        public virtual void AfterCreate<TResource>(IEnumerable<TResource> entities, ResourcePipeline pipeline) where TResource : class, IIdentifiable
        {
            if (GetHook(ResourceHook.AfterCreate, entities, out var container, out var node))
            {
                container.AfterCreate((HashSet<TResource>)node.UniqueEntities, pipeline);
            }

            Traverse(_traversalHelper.CreateNextLayer(node),
                ResourceHook.AfterUpdateRelationship,
                (nextContainer, nextNode) => FireAfterUpdateRelationship(nextContainer, nextNode, pipeline));
        }

        /// <inheritdoc/>
        public virtual void AfterUpdate<TResource>(IEnumerable<TResource> entities, ResourcePipeline pipeline) where TResource : class, IIdentifiable
        {
            if (GetHook(ResourceHook.AfterUpdate, entities, out var container, out var node))
            {
                container.AfterUpdate((HashSet<TResource>)node.UniqueEntities, pipeline);
            }

            Traverse(_traversalHelper.CreateNextLayer(node),
                ResourceHook.AfterUpdateRelationship,
                (nextContainer, nextNode) => FireAfterUpdateRelationship(nextContainer, nextNode, pipeline));
        }

        /// <inheritdoc/>
        public virtual void AfterDelete<TResource>(IEnumerable<TResource> entities, ResourcePipeline pipeline, bool succeeded) where TResource : class, IIdentifiable
        {
            if (GetHook(ResourceHook.AfterDelete, entities, out var container, out var node))
            {
                container.AfterDelete((HashSet<TResource>)node.UniqueEntities, pipeline, succeeded);
            }
        }

        /// <summary>
        /// For a given <see cref="ResourceHook"/> target and for a given type 
        /// <typeparamref name="TResource"/>, gets the hook container if the target
        /// hook was implemented and should be executed.
        /// <para />
        /// Along the way, creates a traversable node from the root entity set.
        /// </summary>
        /// <returns><c>true</c>, if hook was implemented, <c>false</c> otherwise.</returns>
        bool GetHook<TResource>(ResourceHook target, IEnumerable<TResource> entities,
            out IResourceHookContainer<TResource> container,
            out RootNode<TResource> node) where TResource : class, IIdentifiable
        {
            node = _traversalHelper.CreateRootNode(entities);
            container = _executorHelper.GetResourceHookContainer<TResource>(target);
            return container != null;
        }

        /// <summary>
        /// Traverses the nodes in a <see cref="NodeLayer"/>.
        /// </summary>
        void Traverse(NodeLayer currentLayer, ResourceHook target, Action<IResourceHookContainer, INode> action)
        {
            if (!currentLayer.AnyEntities()) return;
            foreach (INode node in currentLayer)
            {
                var entityType = node.EntityType;
                var hookContainer = _executorHelper.GetResourceHookContainer(entityType, target);
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
        void RecursiveBeforeRead(List<RelationshipAttribute> relationshipChain, ResourcePipeline pipeline, List<PrincipalType> calledContainers)
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
        /// Fires the nested before hooks for entities in the current <paramref name="layer"/>
        /// </summary>
        /// <remarks>
        /// For example: consider the case when the owner of article1 (one-to-one) 
        /// is being updated from owner_old to owner_new, where owner_new is currently already 
        /// related to article2. Then, the following nested hooks need to be fired in the following order. 
        /// First the BeforeUpdateRelationship should be for owner1, then the 
        /// BeforeImplicitUpdateRelationship hook should be fired for
        /// owner2, and lastely the BeforeImplicitUpdateRelationship for article2.</remarks>
        void FireNestedBeforeUpdateHooks(ResourcePipeline pipeline, NodeLayer layer)
        {
            foreach (INode node in layer)
            {
                var nestedHookcontainer = _executorHelper.GetResourceHookContainer(node.EntityType, ResourceHook.BeforeUpdateRelationship);
                IEnumerable uniqueEntities = node.UniqueEntities;
                DependentType entityType = node.EntityType;
                Dictionary<RelationshipAttribute, IEnumerable> currenEntitiesGrouped;
                Dictionary<RelationshipAttribute, IEnumerable> currentEntitiesGroupedInverse;

                // fire the BeforeUpdateRelationship hook for owner_new
                if (nestedHookcontainer != null)
                {
                    if (uniqueEntities.Cast<IIdentifiable>().Any())
                    {
                        var relationships = node.RelationshipsToNextLayer.Select(p => p.Attribute).ToArray();
                        var dbValues = LoadDbValues(entityType, uniqueEntities, ResourceHook.BeforeUpdateRelationship, relationships);

                        /// these are the entities of the current node grouped by 
                        /// RelationshipAttributes that occured in the previous layer
                        /// so it looks like { HasOneAttribute:owner  =>  owner_new }.
                        /// Note that in the BeforeUpdateRelationship hook of Person, 
                        /// we want want inverse relationship attribute:
                        /// we now have the one pointing from article -> person, ]
                        /// but we require the the one that points from person -> article             
                        currenEntitiesGrouped = node.RelationshipsFromPreviousLayer.GetDependentEntities();
                        currentEntitiesGroupedInverse = ReplaceKeysWithInverseRelationships(currenEntitiesGrouped);

                        var resourcesByRelationship = CreateRelationshipHelper(entityType, currentEntitiesGroupedInverse, dbValues);
                        var allowedIds = CallHook(nestedHookcontainer, ResourceHook.BeforeUpdateRelationship, new object[] { GetIds(uniqueEntities), resourcesByRelationship, pipeline }).Cast<string>();
                        var updated = GetAllowedEntities(uniqueEntities, allowedIds);
                        node.UpdateUnique(updated);
                        node.Reassign();
                    }
                }

                /// Fire the BeforeImplicitUpdateRelationship hook for owner_old.
                /// Note: if the pipeline is Post it means we just created article1,
                /// which means we are sure that it isn't related to any other entities yet.
                if (pipeline != ResourcePipeline.Post)
                {
                    /// To fire a hook for owner_old, we need to first get a reference to it.
                    /// For this, we need to query the database for the  HasOneAttribute:owner 
                    /// relationship of article1, which is referred to as the 
                    /// principal side of the HasOneAttribute:owner relationship.
                    var principalEntities = node.RelationshipsFromPreviousLayer.GetPrincipalEntities();
                    if (principalEntities.Any())
                    {
                        /// owner_old is loaded, which is an "implicitly affected entity"
                        FireForAffectedImplicits(entityType, principalEntities, pipeline, uniqueEntities);
                    }
                }

                /// Fire the BeforeImplicitUpdateRelationship hook for article2
                /// For this, we need to query the database for the current owner 
                /// relationship value of owner_new.
                currenEntitiesGrouped = node.RelationshipsFromPreviousLayer.GetDependentEntities();
                if (currenEntitiesGrouped.Any())
                {
                    /// dependentEntities is grouped by relationships from previous 
                    /// layer, ie { HasOneAttribute:owner  =>  owner_new }. But 
                    /// to load article2 onto owner_new, we need to have the 
                    /// RelationshipAttribute from owner to article, which is the
                    /// inverse of HasOneAttribute:owner
                    currentEntitiesGroupedInverse = ReplaceKeysWithInverseRelationships(currenEntitiesGrouped);
                    /// Note that currently in the JADNC implementation of hooks, 
                    /// the root layer is ALWAYS homogenous, so we safely assume 
                    /// that for every relationship to the previous layer, the 
                    /// principal type is the same.
                    PrincipalType principalEntityType = currenEntitiesGrouped.First().Key.LeftType;
                    FireForAffectedImplicits(principalEntityType, currentEntitiesGroupedInverse, pipeline);
                }
            }
        }

        /// <summary>
        /// replaces the keys of the <paramref name="entitiesByRelationship"/> dictionary
        /// with its inverse relationship attribute.
        /// </summary>
        /// <param name="entitiesByRelationship">Entities grouped by relationship attribute</param>
        Dictionary<RelationshipAttribute, IEnumerable> ReplaceKeysWithInverseRelationships(Dictionary<RelationshipAttribute, IEnumerable> entitiesByRelationship)
        {
            /// when Article has one Owner (HasOneAttribute:owner) is set, there is no guarantee
            /// that the inverse attribute was also set (Owner has one Article: HasOneAttr:article).
            /// If it isn't, JADNC currently knows nothing about this relationship pointing back, and it 
            /// currently cannot fire hooks for entities resolved through inverse relationships.
            var inversableRelationshipAttributes = entitiesByRelationship.Where(kvp => kvp.Key.InverseNavigation != null);
            return inversableRelationshipAttributes.ToDictionary(kvp => _resourceGraph.GetInverse(kvp.Key), kvp => kvp.Value);
        }

        /// <summary>
        /// Given a source of entities, gets the implicitly affected entities 
        /// from the database and calls the BeforeImplicitUpdateRelationship hook.
        /// </summary>
        void FireForAffectedImplicits(Type entityTypeToInclude, Dictionary<RelationshipAttribute, IEnumerable> implicitsTarget, ResourcePipeline pipeline, IEnumerable existingImplicitEntities = null)
        {
            var container = _executorHelper.GetResourceHookContainer(entityTypeToInclude, ResourceHook.BeforeImplicitUpdateRelationship);
            if (container == null) return;
            var implicitAffected = _executorHelper.LoadImplicitlyAffected(implicitsTarget, existingImplicitEntities);
            if (!implicitAffected.Any()) return;
            var inverse = implicitAffected.ToDictionary(kvp => _resourceGraph.GetInverse(kvp.Key), kvp => kvp.Value);
            var resourcesByRelationship = CreateRelationshipHelper(entityTypeToInclude, inverse);
            CallHook(container, ResourceHook.BeforeImplicitUpdateRelationship, new object[] { resourcesByRelationship, pipeline, });
        }

        /// <summary>
        /// checks that the collection does not contain more than one item when
        /// relevant (eg AfterRead from GetSingle pipeline).
        /// </summary>
        /// <param name="returnedList"> The collection returned from the hook</param>
        /// <param name="pipeline">The pipeine from which the hook was fired</param>
        void ValidateHookResponse<T>(IEnumerable<T> returnedList, ResourcePipeline pipeline = 0)
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
        IEnumerable CallHook(IResourceHookContainer container, ResourceHook hook, object[] arguments)
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
        object ThrowJsonApiExceptionOnError(Func<object> action)
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
        /// Helper method to instantiate AffectedRelationships for a given <paramref name="entityType"/>
        /// If <paramref name="dbValues"/> are included, the values of the entries in <paramref name="prevLayerRelationships"/> need to be replaced with these values.
        /// </summary>
        /// <returns>The relationship helper.</returns>
        IRelationshipsDictionary CreateRelationshipHelper(DependentType entityType, Dictionary<RelationshipAttribute, IEnumerable> prevLayerRelationships, IEnumerable dbValues = null)
        {
            if (dbValues != null) prevLayerRelationships = ReplaceWithDbValues(prevLayerRelationships, dbValues.Cast<IIdentifiable>());
            return (IRelationshipsDictionary)TypeHelper.CreateInstanceOfOpenType(typeof(RelationshipsDictionary<>), entityType, true, prevLayerRelationships);
        }

        /// <summary>
        /// Replaces the entities in the values of the prevLayerRelationships dictionary 
        /// with the corresponding entities loaded from the db.
        /// </summary>
        Dictionary<RelationshipAttribute, IEnumerable> ReplaceWithDbValues(Dictionary<RelationshipAttribute, IEnumerable> prevLayerRelationships, IEnumerable<IIdentifiable> dbValues)
        {
            foreach (var key in prevLayerRelationships.Keys.ToList())
            {
                var replaced = prevLayerRelationships[key].Cast<IIdentifiable>().Select(entity => dbValues.Single(dbEntity => dbEntity.StringId == entity.StringId)).Cast(key.LeftType);
                prevLayerRelationships[key] = TypeHelper.CreateHashSetFor(key.LeftType, replaced);
            }
            return prevLayerRelationships;
        }

        /// <summary>
        /// Fitler the source set by removing the entities with id that are not 
        /// in <paramref name="allowedIds"/>.
        /// </summary>
        HashSet<IIdentifiable> GetAllowedEntities(IEnumerable source, IEnumerable<string> allowedIds)
        {
            return new HashSet<IIdentifiable>(source.Cast<IIdentifiable>().Where(ue => allowedIds.Contains(ue.StringId)));
        }

        /// <summary>
        /// given the set of <paramref name="uniqueEntities"/>, it will load all the 
        /// values from the database of these entites.
        /// </summary>
        /// <returns>The db values.</returns>
        /// <param name="entityType">type of the entities to be loaded</param>
        /// <param name="uniqueEntities">The set of entities to load the db values for</param>
        /// <param name="targetHook">The hook in which the db values will be displayed.</param>
        /// <param name="relationshipsToNextLayer">Relationships from <paramref name="entityType"/> to the next layer: 
        /// this indicates which relationships will be included on <paramref name="uniqueEntities"/>.</param>
        IEnumerable LoadDbValues(Type entityType, IEnumerable uniqueEntities, ResourceHook targetHook, RelationshipAttribute[] relationshipsToNextLayer)
        {
            /// We only need to load database values if the target hook of this hook execution
            /// cycle is compatible with displaying database values and has this option enabled.
            if (!_executorHelper.ShouldLoadDbValues(entityType, targetHook)) return null;
            return _executorHelper.LoadDbValues(entityType, uniqueEntities, targetHook, relationshipsToNextLayer);
        }

        /// <summary>
        /// Fires the AfterUpdateRelationship hook
        /// </summary>
        void FireAfterUpdateRelationship(IResourceHookContainer container, INode node, ResourcePipeline pipeline)
        {

            Dictionary<RelationshipAttribute, IEnumerable> currenEntitiesGrouped = node.RelationshipsFromPreviousLayer.GetDependentEntities();
            /// the relationships attributes in currenEntitiesGrouped will be pointing from a 
            /// resource in the previouslayer to a resource in the current (nested) layer.
            /// For the nested hook we need to replace these attributes with their inverse.
            /// See the FireNestedBeforeUpdateHooks method for a more detailed example.
            var resourcesByRelationship = CreateRelationshipHelper(node.EntityType, ReplaceKeysWithInverseRelationships(currenEntitiesGrouped));
            CallHook(container, ResourceHook.AfterUpdateRelationship, new object[] { resourcesByRelationship, pipeline });
        }

        /// <summary>
        /// Returns a list of StringIds from a list of IIdentifiables (<paramref name="entities"/>).
        /// </summary>
        /// <returns>The ids.</returns>
        /// <param name="entities">iidentifiable entities.</param>
        HashSet<string> GetIds(IEnumerable entities)
        {
            return new HashSet<string>(entities.Cast<IIdentifiable>().Select(e => e.StringId));
        }
    }
}

