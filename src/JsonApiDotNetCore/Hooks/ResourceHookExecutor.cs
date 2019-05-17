using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using PrincipalType = System.Type;
using DependentType = System.Type;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCore.Extensions;

namespace JsonApiDotNetCore.Hooks
{
    /// <inheritdoc/>
    internal class ResourceHookExecutor : IResourceHookExecutor
    {
        public static readonly IdentifiableComparer Comparer = new IdentifiableComparer();
        internal readonly TraversalHelper _traversalHelper;
        internal readonly IHookExecutorHelper _executorHelper;
        protected readonly IJsonApiContext _context;
        private readonly IResourceGraph _graph;

        public ResourceHookExecutor(IHookExecutorHelper helper, IJsonApiContext context, IResourceGraph graph)
        {
            _executorHelper = helper;
            _context = context;
            _graph = graph;
            _traversalHelper = new TraversalHelper(graph, _context);
        }

        public virtual void BeforeRead<TEntity>(ResourceAction pipeline, string stringId = null) where TEntity : class, IIdentifiable
        {
            var hookContainer = _executorHelper.GetResourceHookContainer<TEntity>(ResourceHook.BeforeRead);
            hookContainer?.BeforeRead(pipeline, false, stringId);
            var contextEntity = _graph.GetContextEntity(typeof(TEntity));
            var calledContainers = new List<PrincipalType>() { typeof(TEntity) };
            foreach (var relationshipPath in _context.IncludedRelationships)
            {
                RecursiveBeforeRead(contextEntity, relationshipPath.Split('.').ToList(), pipeline, calledContainers);
            }
        }

        void RecursiveBeforeRead(ContextEntity contextEntity, List<string> relationshipChain, ResourceAction pipeline, List<PrincipalType> calledContainers)
        {
            var target = relationshipChain.First();
            var relationship = contextEntity.Relationships.FirstOrDefault(r => r.PublicRelationshipName == target);
            if (relationship == null)
            {
                throw new JsonApiException(400, $"Invalid relationship {target} on {contextEntity.EntityName}",
                    $"{contextEntity.EntityName} does not have a relationship named {target}");
            }

            if (!calledContainers.Contains(relationship.DependentType))
            {
                calledContainers.Add(relationship.DependentType);
                var container = _executorHelper.GetResourceHookContainer(relationship.DependentType, ResourceHook.BeforeRead);
                if (container != null)
                {
                    CallHook(container, ResourceHook.BeforeRead, new object[] { pipeline, true, null });
                }
            }
            relationshipChain.RemoveAt(0);
            if (relationshipChain.Any())
            {

                RecursiveBeforeRead(_graph.GetContextEntity(relationship.DependentType), relationshipChain, pipeline, calledContainers);
            }
        }

        public virtual IEnumerable<TEntity> BeforeUpdate<TEntity>(IEnumerable<TEntity> entities, ResourceAction pipeline) where TEntity : class, IIdentifiable
        {
            var hookContainer = _executorHelper.GetResourceHookContainer<TEntity>(ResourceHook.BeforeUpdate);
            var node = _traversalHelper.CreateRootNode(entities);
            if (hookContainer != null)
            {
                var dbValues = _executorHelper.LoadDbValues((IEnumerable<TEntity>)node.UniqueEntities, ResourceHook.BeforeUpdate, node.RelationshipsToNextLayer);
                var diff = new EntityDiff<TEntity>(node.UniqueEntities, dbValues);
                IEnumerable<TEntity> updated = hookContainer.BeforeUpdate(diff, pipeline);
                node.UpdateUnique(updated);
                node.Reassign(entities);
            }

            BeforeUpdateRelationship(pipeline, _traversalHelper.CreateNextLayer(node));
            return entities;
        }

        public virtual IEnumerable<TEntity> BeforeCreate<TEntity>(IEnumerable<TEntity> entities, ResourceAction pipeline) where TEntity : class, IIdentifiable
        {
            var hookContainer = _executorHelper.GetResourceHookContainer<TEntity>(ResourceHook.BeforeCreate);
            var node = _traversalHelper.CreateRootNode(entities);
            if (hookContainer != null)
            {
                IEnumerable<TEntity> updated = hookContainer.BeforeCreate((HashSet<TEntity>)node.UniqueEntities, pipeline);
                node.UpdateUnique(updated);
                node.Reassign(entities);
            }
            BeforeUpdateRelationship(pipeline, _traversalHelper.CreateNextLayer(node));
            return entities;
        }

        void BeforeUpdateRelationship(ResourceAction pipeline, EntityChildLayer layer)
        {
            foreach (IEntityNode node in layer)
            {
                var nestedHookcontainer = _executorHelper.GetResourceHookContainer(node.EntityType, ResourceHook.BeforeUpdateRelationship);
                IEnumerable uniqueEntities = node.UniqueEntities;
                DependentType entityType = node.EntityType;

                if (nestedHookcontainer != null)
                {
                    if (uniqueEntities.Cast<IIdentifiable>().Any())
                    {
                        var dbValues = _executorHelper.LoadDbValues(entityType, uniqueEntities, ResourceHook.BeforeUpdateRelationship, node.RelationshipsToNextLayer);
                        var relationshipHelper = CreateRelationshipHelper(entityType, node.RelationshipsFromPreviousLayer.GetDependentEntities(), dbValues);
                        var allowedIds = CallHook(nestedHookcontainer, ResourceHook.BeforeUpdateRelationship, new object[] { GetIds(uniqueEntities), relationshipHelper, pipeline }).Cast<string>();
                        var updated = GetAllowedEntities(uniqueEntities, allowedIds);
                        node.UpdateUnique(updated);
                        node.Reassign();
                    }
                }

                var implicitPrincipalTargets = node.RelationshipsFromPreviousLayer.GetPrincipalEntities();
                if (pipeline != ResourceAction.Create && implicitPrincipalTargets.Any())
                {
                    FireForAffectedImplicits(entityType, implicitPrincipalTargets, pipeline, uniqueEntities);
                }

                var dependentEntities = node.RelationshipsFromPreviousLayer.GetDependentEntities();
                if (dependentEntities.Any())
                {
                    (var implicitDependentTargets, var principalEntityType) = GetDependentImplicitsTargets(dependentEntities);
                    FireForAffectedImplicits(principalEntityType, implicitDependentTargets, pipeline);
                }
            }
        }

        void FireForAffectedImplicits(Type entityType, Dictionary<RelationshipProxy, IEnumerable> implicitsTarget, ResourceAction pipeline, IEnumerable existingImplicitEntities = null)
        {
            var container = _executorHelper.GetResourceHookContainer(entityType, ResourceHook.BeforeImplicitUpdateRelationship);
            if (container == null) return;
            var implicitAffected = _executorHelper.LoadImplicitlyAffected(implicitsTarget, existingImplicitEntities);
            if (!implicitAffected.Any()) return;
            var relationshipHelper = CreateRelationshipHelper(entityType, implicitAffected);
            CallHook(container, ResourceHook.BeforeImplicitUpdateRelationship, new object[] { relationshipHelper, pipeline, });
        }

        /// <summary>
        /// NOTE: in JADNC usage, the root layer is ALWAYS homogenous, so we can be sure that for every 
        /// relationship to the previous layer, the principal type is the same.
        /// </summary>
        /// <returns>The dependent implicits targets.</returns>
        /// <param name="node">Node.</param>
        (Dictionary<RelationshipProxy, IEnumerable>, PrincipalType) GetDependentImplicitsTargets(Dictionary<RelationshipProxy, IEnumerable> dependentEntities)
        {
            PrincipalType principalType = dependentEntities.First().Key.PrincipalType;
            var byInverseRelationship = dependentEntities.Where(kvp => kvp.Key.Attribute.InverseNavigation != null).ToDictionary(kvp => GetInverseRelationship(kvp.Key), kvp => kvp.Value);
            return (byInverseRelationship, principalType);

        }

        public virtual IEnumerable<TEntity> BeforeDelete<TEntity>(IEnumerable<TEntity> entities, ResourceAction pipeline) where TEntity : class, IIdentifiable
        {
            var hookContainer = _executorHelper.GetResourceHookContainer<TEntity>(ResourceHook.BeforeDelete);
            var node = _traversalHelper.CreateRootNode(entities);
            if (hookContainer != null)
            {
                IEnumerable<TEntity> updated = hookContainer.BeforeDelete((HashSet<TEntity>)node.UniqueEntities, pipeline);
                node.UpdateUnique(updated);
                node.Reassign(entities);
            }


            foreach (var implicitTargets in node.RelationshipsToNextLayerByType())
            {
                var dependentType = implicitTargets.First().Key.DependentType;
                FireForAffectedImplicits(dependentType, implicitTargets, pipeline);
            }
            return entities;
        }

        private object GroupByDependent<TEntity>(RelationshipProxy[] relationships, IEnumerable<TEntity> entities) where TEntity : class, IIdentifiable
        {
            return relationships.GroupBy(proxy => proxy.DependentType).ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());

        }

        public virtual IEnumerable<TEntity> OnReturn<TEntity>(IEnumerable<TEntity> entities, ResourceAction pipeline) where TEntity : class, IIdentifiable
        {
            var hookContainer = _executorHelper.GetResourceHookContainer<TEntity>(ResourceHook.OnReturn);
            var node = _traversalHelper.CreateRootNode(entities);
            if (hookContainer != null && pipeline != ResourceAction.GetRelationship)
            {
                IEnumerable<TEntity> updated = hookContainer.OnReturn((HashSet<TEntity>)node.UniqueEntities, pipeline);
                ValidateHookResponse(updated);
                node.UpdateUnique(updated);
                node.Reassign(entities);
            }

            Traverse(_traversalHelper.CreateNextLayer(node), ResourceHook.OnReturn, (container, nextNode) =>
            {
                var filteredUniqueSet = CallHook(container, ResourceHook.OnReturn, new object[] { nextNode.UniqueEntities, pipeline });
                nextNode.UpdateUnique(filteredUniqueSet);
                nextNode.Reassign();
            });
            return entities;
        }

        public virtual void AfterRead<TEntity>(IEnumerable<TEntity> entities, ResourceAction pipeline) where TEntity : class, IIdentifiable
        {
            var hookContainer = _executorHelper.GetResourceHookContainer<TEntity>(ResourceHook.AfterRead);
            var node = _traversalHelper.CreateRootNode(entities);
            if (hookContainer != null)
            {
                hookContainer.AfterRead((HashSet<TEntity>)node.UniqueEntities, pipeline);
            }

            Traverse(_traversalHelper.CreateNextLayer(node), ResourceHook.AfterRead, (container, nextNode) =>
            {
                CallHook(container, ResourceHook.AfterRead, new object[] { nextNode.UniqueEntities, pipeline, true });
            });
        }

        public virtual void AfterCreate<TEntity>(IEnumerable<TEntity> entities, ResourceAction pipeline) where TEntity : class, IIdentifiable
        {
            var hookContainer = _executorHelper.GetResourceHookContainer<TEntity>(ResourceHook.AfterCreate);
            var node = _traversalHelper.CreateRootNode(entities);
            if (hookContainer != null)
            {
                hookContainer.AfterCreate((HashSet<TEntity>)node.UniqueEntities, pipeline);
            }
            Traverse(_traversalHelper.CreateNextLayer(node), ResourceHook.AfterUpdateRelationship, (container, nextNode) => AfterUpdateRelationship(container, nextNode, pipeline));
        }

        public virtual void AfterUpdate<TEntity>(IEnumerable<TEntity> entities, ResourceAction pipeline) where TEntity : class, IIdentifiable
        {
            var hookContainer = _executorHelper.GetResourceHookContainer<TEntity>(ResourceHook.AfterUpdate);
            var node = _traversalHelper.CreateRootNode(entities);
            if (hookContainer != null)
            {
                hookContainer.AfterUpdate((HashSet<TEntity>)node.UniqueEntities, pipeline);
            }
            Traverse(_traversalHelper.CreateNextLayer(node), ResourceHook.AfterUpdateRelationship, (container, nextNode) => AfterUpdateRelationship(container, nextNode, pipeline));
        }

        void Traverse(EntityChildLayer currentLayer, ResourceHook target, Action<IResourceHookContainer, IEntityNode> action)
        {
            if (!currentLayer.AnyEntities()) return;
            foreach (IEntityNode node in currentLayer)
            {
                var entityType = node.EntityType;
                var hookContainer = _executorHelper.GetResourceHookContainer(entityType, target);
                if (hookContainer == null) continue;
                action(hookContainer, node);
            }

            Traverse(_traversalHelper.CreateNextLayer(currentLayer.ToList()), target, action);
        }

        void AfterUpdateRelationship(IResourceHookContainer container, IEntityNode node, ResourceAction pipeline)
        {
            var relationshipHelper = CreateRelationshipHelper(node.EntityType, node.RelationshipsFromPreviousLayer.GetDependentEntities());
            CallHook(container, ResourceHook.AfterUpdateRelationship, new object[] { relationshipHelper, pipeline });
        }

        public virtual void AfterDelete<TEntity>(IEnumerable<TEntity> entities, ResourceAction pipeline, bool succeeded) where TEntity : class, IIdentifiable
        {
            var hookContainer = _executorHelper.GetResourceHookContainer<TEntity>(ResourceHook.AfterDelete);
            var node = _traversalHelper.CreateRootNode(entities);
            if (hookContainer != null)
            {
                hookContainer.AfterDelete((HashSet<TEntity>)node.UniqueEntities, pipeline, succeeded);
            }
        }

        /// <summary>
        /// checks that the collection does not contain more than one item when
        /// relevant (eg AfterRead from GetSingle pipeline).
        /// </summary>
        /// <param name="returnedList"> The collection returned from the hook</param>
        /// <param name="pipeline">The pipeine from which the hook was fired</param>
        void ValidateHookResponse<T>(IEnumerable<T> returnedList, ResourceAction pipeline = 0)
        {
            if (pipeline == ResourceAction.GetSingle && returnedList.Count() > 1)
            {
                throw new ApplicationException("The returned collection from this hook may contain at most one item in the case of the" +
                    pipeline.ToString("G") + "pipeline");
            }
        }


        IEnumerable CallHook(IResourceHookContainer container, ResourceHook hook, object[] arguments)
        {
            var method = container.GetType().GetMethod(hook.ToString("G"));
            // note that some of the hooks return "void". When these hooks, the 
            // are called reflectively with Invoke like here, the return value
            // is just null, so we don't have to worry about casting issues here.
            return (IEnumerable)ThrowJsonApiExceptionOnError(() => method.Invoke(container, arguments));
        }

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
        /// Helper method to instantiate UpdatedRelationshipHelper for a given <paramref name="entityType"/>
        /// If <paramref name="dbValues"/> are included, the values of the entries in <paramref name="prevLayerRelationships"/> need to be replaced with these values.
        /// </summary>
        /// <returns>The relationship helper.</returns>
        /// <param name="entityType">Entity type.</param>
        /// <param name="prevLayerRelationships">Previous layer relationships.</param>
        /// <param name="dbValues">Db values.</param>
        IUpdatedRelationshipHelper CreateRelationshipHelper(DependentType entityType, Dictionary<RelationshipProxy, IEnumerable> prevLayerRelationships, IEnumerable dbValues = null)
        {
            if (dbValues != null) ReplaceWithDbValues(prevLayerRelationships, dbValues.Cast<IIdentifiable>());
            return (IUpdatedRelationshipHelper)TypeHelper.CreateInstanceOfOpenType(typeof(UpdatedRelationshipHelper<>), entityType, prevLayerRelationships);
        }

        void ReplaceWithDbValues(Dictionary<RelationshipProxy, IEnumerable> prevLayerRelationships, IEnumerable<IIdentifiable> dbValues)
        {
            foreach ( var key in prevLayerRelationships.Keys.ToList())
            {
                var replaced = prevLayerRelationships[key].Cast<IIdentifiable>().Select(entity => dbValues.Single(dbEntity => dbEntity.StringId == entity.StringId)).Cast(key.DependentType);
                prevLayerRelationships[key] = replaced;
            }
        }

        HashSet<string> GetIds(IEnumerable entities)
        {
            return new HashSet<string>(entities.Cast<IIdentifiable>().Select(e => e.StringId));
        }

        HashSet<IIdentifiable> GetAllowedEntities(IEnumerable source, IEnumerable<string> allowedIds)
        {
            return new HashSet<IIdentifiable>(source.Cast<IIdentifiable>().Where(ue => allowedIds.Contains(ue.StringId)));
        }

        RelationshipProxy GetInverseRelationship(RelationshipProxy proxy)
        {
            return new RelationshipProxy(_graph.GetInverseRelationship(proxy.Attribute), proxy.PrincipalType, false);
        }
    }
}

