using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    /// <inheritdoc/>
    public class ResourceHookExecutor : IResourceHookExecutor
    {
        public static readonly ResourceAction[] SingleActions =
        {
            ResourceAction.GetSingle,
            ResourceAction.Create,
            ResourceAction.Delete,
            ResourceAction.Patch,
            ResourceAction.GetRelationship,
            ResourceAction.PatchRelationship
        };
        public static readonly ResourceHook[] ImplicitUpdateHooks =
        {
            ResourceHook.BeforeCreate,
            ResourceHook.BeforeUpdate,
            ResourceHook.BeforeDelete
        };
        protected readonly IHookExecutorHelper _meta;
        protected Dictionary<Type, HashSet<IIdentifiable>> _processedEntities;
        public ResourceHookExecutor(IHookExecutorHelper meta)
        {
            _meta = meta;
            _processedEntities = new Dictionary<Type, HashSet<IIdentifiable>>();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> BeforeCreate<TEntity>(IEnumerable<TEntity> entities, ResourceAction actionSource) where TEntity : class, IIdentifiable
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.BeforeCreate);
            if (hookContainer != null)
            {
                RegisterProcessedEntities(entities);

                var dbEntities = _meta.GetDatabaseValues(hookContainer, entities, ResourceHook.BeforeCreate);
                var context = new HookExecutionContext<TEntity>(actionSource);
                var diff = new EntityDiff<TEntity>(new HashSet<TEntity>(entities), dbEntities);
                var parsedEntities = hookContainer.BeforeCreate(diff, context);
                ValidateHookResponse(parsedEntities, actionSource);
                entities = parsedEntities;
            }

            BreadthFirstTraverse(entities, (container, layerNode) =>
            {

                var uniqueSet = layerNode.UniqueSet;
                var dbEntities = _meta.GetDatabaseValues(hookContainer, uniqueSet, ResourceHook.BeforeUpdate, layerNode.DependentType);
                var context = TypeHelper.CreateInstanceOfOpenType(typeof(HookExecutionContext<>), layerNode.DependentType, actionSource, layerNode.RelationshipGroups);
                var diff = TypeHelper.CreateInstanceOfOpenType(typeof(EntityDiff<>), layerNode.DependentType, layerNode.UniqueSet, null);
                return CallHook(container, ResourceHook.BeforeUpdate, new object[] { diff, context });
            }, ResourceHook.BeforeUpdate);

            FlushRegister();
            return entities;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> AfterCreate<TEntity>(IEnumerable<TEntity> entities, ResourceAction actionSource) where TEntity : class, IIdentifiable
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.AfterCreate);
            if (hookContainer != null)
            {
                RegisterProcessedEntities(entities);
                var context = new HookExecutionContext<TEntity>(actionSource);
                var parsedEntities = hookContainer.AfterCreate(entities, context);
                ValidateHookResponse(parsedEntities, actionSource);
                entities = parsedEntities;
            }

            BreadthFirstTraverse(entities, (container, layerNode) =>
            {
                var context = TypeHelper.CreateInstanceOfOpenType(typeof(HookExecutionContext<>), layerNode.DependentType, actionSource, null);
                return CallHook(container, ResourceHook.AfterUpdate, new object[] { layerNode.UniqueSet, context });
            }, ResourceHook.AfterUpdate);

            FlushRegister();
            return entities;
        }

        /// <inheritdoc/>
        public virtual void BeforeRead<TEntity>(ResourceAction actionSource, string stringId = null) where TEntity : class, IIdentifiable
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.BeforeRead);
            var context = new HookExecutionContext<TEntity>(actionSource);
            hookContainer?.BeforeRead(context, stringId);
            FlushRegister();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> AfterRead<TEntity>(IEnumerable<TEntity> entities, ResourceAction actionSource) where TEntity : class, IIdentifiable
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.AfterRead);
            if (hookContainer != null)
            {
                RegisterProcessedEntities(entities);
                var context = new HookExecutionContext<TEntity>(actionSource);
                var parsedEntities = hookContainer.AfterRead(entities, context);
                ValidateHookResponse(parsedEntities, actionSource);
                entities = parsedEntities;
            }

            BreadthFirstTraverse(entities, (container, layerNode) =>
            {
                var dependentType = layerNode.DependentType;
                if (_meta.ShouldExecuteHook(dependentType, ResourceHook.BeforeRead))
                {
                    var context = TypeHelper.CreateInstanceOfOpenType(typeof(HookExecutionContext<>), layerNode.DependentType, actionSource, null);

                    CallHook(container, ResourceHook.BeforeRead, new object[] { context, default(string) });
                }

                var uniqueSet = layerNode.UniqueSet;
                if (_meta.ShouldExecuteHook(dependentType, ResourceHook.AfterRead))
                {
                    var context = TypeHelper.CreateInstanceOfOpenType(typeof(HookExecutionContext<>), layerNode.DependentType, actionSource, null);
                    return CallHook(container, ResourceHook.AfterRead, new object[] { uniqueSet, context });
                }
                return uniqueSet;
            }, ResourceHook.AfterRead, ResourceHook.BeforeRead);

            FlushRegister();
            return entities;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> BeforeUpdate<TEntity>(IEnumerable<TEntity> entities, ResourceAction actionSource) where TEntity : class, IIdentifiable
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.BeforeUpdate);
            if (hookContainer != null)
            {
                RegisterProcessedEntities(entities);

                var dbEntities = _meta.GetDatabaseValues(hookContainer, entities, ResourceHook.BeforeUpdate);
                var context = new HookExecutionContext<TEntity>(actionSource);
                var diff = new EntityDiff<TEntity>(new HashSet<TEntity>(entities), dbEntities);
                var parsedEntities = hookContainer.BeforeUpdate(diff, context);
                ValidateHookResponse(parsedEntities, actionSource);
                entities = parsedEntities;
            }

            BreadthFirstTraverse(entities, (container, layerNode) =>
            {
                var uniqueSet = layerNode.UniqueSet;
                var dbEntities = _meta.GetDatabaseValues(hookContainer, uniqueSet, ResourceHook.BeforeUpdate, layerNode.DependentType);
                var context = TypeHelper.CreateInstanceOfOpenType(typeof(HookExecutionContext<>), layerNode.DependentType, actionSource, layerNode.RelationshipGroups);
                var diff = TypeHelper.CreateInstanceOfOpenType(typeof(EntityDiff<>), layerNode.DependentType, uniqueSet, dbEntities);

                return CallHook(container, ResourceHook.BeforeUpdate, new object[] { diff, context });
            }, ResourceHook.BeforeUpdate);

            FlushRegister();
            return entities;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> AfterUpdate<TEntity>(IEnumerable<TEntity> entities, ResourceAction actionSource) where TEntity : class, IIdentifiable
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.AfterUpdate);
            if (hookContainer != null)
            {
                RegisterProcessedEntities(entities);
                var context = new HookExecutionContext<TEntity>(actionSource);
                var parsedEntities = hookContainer.AfterUpdate(entities, context);
                ValidateHookResponse(parsedEntities, actionSource);
                entities = parsedEntities;
            }

            BreadthFirstTraverse(entities, (container, layerNode) =>
            {
                var context = TypeHelper.CreateInstanceOfOpenType(typeof(HookExecutionContext<>), layerNode.DependentType, actionSource, null);
                return CallHook(container, ResourceHook.AfterUpdate, new object[] { layerNode.UniqueSet, context });
            }, ResourceHook.AfterUpdate);

            FlushRegister();
            return entities;
        }

        /// <inheritdoc/>
        public virtual void BeforeDelete<TEntity>(IEnumerable<TEntity> entities, ResourceAction actionSource) where TEntity : class, IIdentifiable
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.BeforeDelete);
            var context = new HookExecutionContext<TEntity>(actionSource);
            hookContainer?.BeforeDelete(entities, context);
            FlushRegister();
        }

        /// <inheritdoc/>
        public virtual void AfterDelete<TEntity>(IEnumerable<TEntity> entities, ResourceAction actionSource, bool succeeded) where TEntity : class, IIdentifiable
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.AfterDelete);
            var context = new HookExecutionContext<TEntity>(actionSource);
            hookContainer?.AfterDelete(entities, context, succeeded);
            FlushRegister();
        }

        /// <summary>
        /// Fires the hooks for related (nested) entities.
        /// Performs a recursive, forward-looking breadth first traversal 
        /// through the entitis in <paramref name="currentEntityTreeLayer"/> and fires the 
        /// associated resource hooks 
        /// </summary>
        /// <param name="currentEntityTreeLayer">Current layer.</param>
        /// <param name="hookExecutionAction">Hook execution action.</param>
        void BreadthFirstTraverse(
            IEnumerable<IIdentifiable> previousLayerEntities,
            Func<IResourceHookContainer, NodeInLayer, IEnumerable> hookExecutionAction,
            params ResourceHook[] targetHooks
            )
        {
            if (!previousLayerEntities.Any()) return;
            var previousLayerTypes = new HashSet<Type>(previousLayerEntities.Select(e => e.GetType()));
            _meta.UpdateMetaInformation(previousLayerTypes, targetHooks);

            /// for the entities in the previous layer, extract the current layer
            /// entities by parsing the populated relationships.
            var currentLayer = ExtractionLoop(previousLayerEntities);

            if (currentLayer.Any()) return;

            // for the unique set of entities in that collection, execute the hooks
            ExecutionLoop(currentLayer, hookExecutionAction);
            // for the entities in the current layer: reassign relationships where needed.
            AssignmentLoop(previousLayerEntities, currentLayer);

            if (ShouldCheckForImplicitUpdates(targetHooks))
                ImplicitUpdateLoop(currentLayer, previousLayerTypes);

            var currentLayerEntities = currentLayer.GetAllUniqueEntities();
            BreadthFirstTraverse(currentLayerEntities, hookExecutionAction, targetHooks);

        }


        /// <summary>
        /// Checks if the current resource hook in this traversal might result in a 
        /// implicit relationship update.
        /// </summary>
        /// <returns>A boolean indiciating whether or not the hook should be fired</returns>
        /// <param name="targetHooks">Target hooks.</param>
        bool ShouldCheckForImplicitUpdates(ResourceHook[] targetHooks)
        {
            return targetHooks.Intersect(ImplicitUpdateHooks).Any();
        }

        /// <summary>
        /// If the resource hook that is fired in this traversal is one that 
        /// could trigger an implicit update, check if the ImplicitUpdateRelationship
        /// hook is implemented, and if so, handle implicit updates.
        /// </summary>
        /// <param name="currentLayer">Current layer entities.</param>
        /// <param name="uniqueTypesInLayer">Unique types in layer.</param>
        void ImplicitUpdateLoop(EntityTreeLayer currentLayer, HashSet<Type> typesInPreviousLayer)
        {
            foreach (var principalEntiyType in typesInPreviousLayer)
            {
                var principalContainer = _meta.GetResourceHookContainer(principalEntiyType, ResourceHook.ImplicitUpdateRelationship);
                /// if the ImplicitUpdateRelationship hook is not implemented, we don't 
                /// have to worry about any of this.
                if (principalContainer == null) return;

                // this will include 1:1 and 1:n relationships attrs to entities in the current layer
                var affectedRelations = _meta.GetRelationshipsToType(principalEntiyType).Where(dt => !(dt.Attribute is HasManyThroughAttribute));

                foreach (RelationshipProxy relationship in affectedRelations)
                {
                    var affectedEntities = currentLayer.GetUniqueFilteredSet(relationship);
                    IList inverseEntities = _meta.GetInverseEntities(affectedEntities, relationship);

                    /// figure out if we even know how to get inverse related
                    /// iterate over affectedEntities, get inverse related
                    /// fire hook for inverse related

                    CallHook(principalContainer, ResourceHook.ImplicitUpdateRelationship, new object[] { inverseEntities, relationship.Attribute });
                }
            }
        }


        /// <summary>
        /// Iterates through the entities in the current layer. This layer can be inhomogeneous.
        /// For each of these entities: gets all related entity  for which we want to 
        /// execute a hook (target entities), this is defined in MetaInfo.
        /// Grouped per relation, stores these target in relationshipsInCurrentEntityTreeLayer
        /// </summary>
        /// <returns>Hook targets for current layer.</returns>
        /// <param name="currentEntityTreeLayer">Current layer.</param>
        EntityTreeLayer ExtractionLoop(
        IEnumerable<IIdentifiable> previousLayerEntities
        )
        {
            var currentLayer = new EntityTreeLayer();
            foreach (IIdentifiable entity in previousLayerEntities)
            {
                var principalType = entity.GetType();
                foreach (RelationshipProxy proxy in _meta.GetRelationshipsToType(principalType))
                {
                    // for every (unique) entity, we get the relationships that
                    // will bring us the entities for the current layer
                    var relationshipValue = proxy.GetValue(entity);
                    // skip this relationship if it's not populated
                    if (!proxy.IsContextRelation && relationshipValue == null) continue;
                    if (!(relationshipValue is IEnumerable<IIdentifiable> currentLayerEntities))
                    {
                        // in the case of a to-one relationship, the assigned value
                        // will not be a list. We therefore first wrap it in a list.
                        var list = TypeHelper.CreateListFor(proxy.DependentType);
                        if (relationshipValue != null) list.Add(relationshipValue);
                        currentLayerEntities = (IEnumerable<IIdentifiable>)list;
                    }
                    // filter the retrieved current layer entities against
                    // the entities that were processed in previous layers, i.e. 
                    // against the set of unique entities in the entire past tree.
                    var newEntitiesInTree = UniqueInTree(currentLayerEntities, proxy.DependentType);
                    currentLayer.Add(currentLayerEntities, proxy, newEntitiesInTree);
                }
            }
            return currentLayer;
        }

        /// <summary>
        /// Executes the hooks for every key in relationshipsInCurrentEntityTreeLayer,
        /// </summary>
        /// <param name="relationshipsInCurrentEntityTreeLayer">Hook targets for current layer.</param>
        /// <param name="hookExecution">Hook execution method.</param>
        void ExecutionLoop(
            EntityTreeLayer currentLayer,
            Func<IResourceHookContainer, NodeInLayer, IEnumerable> hookExecution
            )
        {
            foreach (NodeInLayer layerNode in currentLayer)
            {
                var hookContainer = _meta.GetResourceHookContainer(layerNode.DependentType);
                var filteredUniqueSet = hookExecution(hookContainer, layerNode);
                layerNode.UpdateUniqueSet(filteredUniqueSet);
            }
        }

        /// <summary>
        /// When this method is called, the values in relationshipsInCurrentEntityTreeLayer
        /// will contain a subset compared to in the DoExtractionLoop call.
        /// We now need to iterate through currentEntityTreeLayer again and remove any of 
        /// their related entities that do not occur in relationshipsInCurrentEntityTreeLayer
        /// </summary>
        /// <param name="currentEntityTreeLayer">Entities in current layer.</param>
        /// <param name="relationshipsInCurrentEntityTreeLayer">Hook targets for current layer.</param>
        void AssignmentLoop(
            IEnumerable<IIdentifiable> previousLayerEntities,
            EntityTreeLayer relatedEntitiesInCurrentEntityTreeLayer
            )
        {
            foreach (IIdentifiable previousLayerEntity in previousLayerEntities)
            {
                var principalType = previousLayerEntity.GetType();
                foreach (RelationshipProxy proxy in _meta.GetRelationshipsToType(principalType))
                {
                    /// if there are no related entities included for 
                    /// currentEntityTreeLayerEntity for this relation, then this key will 
                    /// not exist, and we may continue to the next.
                    var parsedEntities = relatedEntitiesInCurrentEntityTreeLayer.GetUniqueFilteredSet(proxy);
                    if (parsedEntities == null) continue;

                    var actualValue = proxy.GetValue(previousLayerEntity);

                    if (actualValue is IEnumerable<IIdentifiable> relationshipCollection)
                    {
                        var convertedCollection = TypeHelper.ConvertCollection(relationshipCollection.Intersect(parsedEntities), proxy.DependentType);
                        proxy.SetValue(previousLayerEntity, convertedCollection);
                    }
                    else if (actualValue is IIdentifiable relationshipSingle)
                    {
                        if (!parsedEntities.Contains(actualValue))
                        {
                            proxy.SetValue(previousLayerEntity, null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// checks that the collection does not contain more than one item when
        /// relevant (eg AfterRead from GetSingle pipeline).
        /// </summary>
        /// <param name="returnedList"> The collection returned from the hook</param>
        /// <param name="actionSource">The pipeine from which the hook was fired</param>
        protected void ValidateHookResponse(object returnedList, ResourceAction actionSource = 0)
        {
            if (actionSource != ResourceAction.None && SingleActions.Contains(actionSource) && ((IEnumerable)returnedList).Cast<object>().Count() > 1)
            {
                throw new ApplicationException("The returned collection from this hook may only contain one item in the case of the" +
                    actionSource.ToString() + "pipeline");
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
        void RegisterProcessedEntities<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IIdentifiable
        {
            RegisterProcessedEntities(entities, typeof(TEntity));
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
        /// A method that reflectively calls a resource hook.
        /// Note: I attempted to cast IResourceHookContainer container to type
        /// IResourceHookContainer{IIdentifiable}, which would have allowed us
        /// to call the hook on the nested containers without reflection, but I 
        /// believe this is not possible. We therefore need this helper method.
        /// </summary>
        /// <returns>The hook.</returns>
        /// <param name="container">Container for related entity.</param>
        /// <param name="hook">Target hook type.</param>
        /// <param name="arguments">Arguments to call the hook with.</param>
        IEnumerable CallHook(IResourceHookContainer container, ResourceHook hook, object[] arguments)
        {
            var method = container.GetType().GetMethod(hook.ToString("G"));
            // note that some of the hooks return "void". When these hooks, the 
            // are called reflectively with Invoke like here, the return value
            // is just null, so we don't have to worry about casting issues here.
            return (IEnumerable)method.Invoke(container, arguments);
        }

        /// <summary>
        /// We need to flush the list of processed entities because typically
        /// the hook executor will be caled twice per service pipeline (eg BeforeCreate
        /// and AfterCreate).
        /// </summary>
        void FlushRegister()
        {
            _processedEntities = new Dictionary<Type, HashSet<IIdentifiable>>();
        }
    }
}

