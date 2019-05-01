using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{

    //if (_meta.RequiresDatabaseDiff(hookContainer, ResourceHook.BeforeUpdate))
    //{
    //    var ids = entities.Select(e => e.StringId).ToArray();
    //    var diffs = repo.Get().Where(e => ids.Contains(e.StringId));
    //    diff = new EntityDiff<TEntity>(entities, diffs);
    //} else

    /// <inheritdoc/>
    public class ResourceHookExecutor : IResourceHookExecutor
    {
        protected readonly IHookExecutorHelper _meta;
        protected readonly ResourceAction[] _singleActions;
        protected Dictionary<Type, HashSet<IIdentifiable>> _processedEntities;
        public ResourceHookExecutor(IHookExecutorHelper meta)
        {
            _meta = meta;
            _processedEntities = new Dictionary<Type, HashSet<IIdentifiable>>();
            _singleActions = new ResourceAction[]
                {
                    ResourceAction.GetSingle,
                    ResourceAction.Create,
                    ResourceAction.Delete,
                    ResourceAction.Patch,
                    ResourceAction.GetRelationship,
                    ResourceAction.PatchRelationship
                };
        }

        private object GetEntityDiffInstance(Type type, object requestEntities, object dbEntities)
        {
            var parameterized = typeof(EntityDiff<>).MakeGenericType(type);
            return Activator.CreateInstance(parameterized, new object[] { requestEntities, dbEntities });
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> BeforeCreate<TEntity>(IEnumerable<TEntity> entities, ResourceAction actionSource) where TEntity : class, IIdentifiable
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.BeforeCreate);
            if (hookContainer != null)
            {
                RegisterProcessedEntities(entities);

                // (var dbEntities, var context) = _meta.GetDatabaseDiff(hookContainer, ResourceHook.BeforeCreate, entities);
                HashSet<TEntity> dbEntities = null;
                HookExecutionContext context = new HookExecutionContext(actionSource);

                var diff = new EntityDiff<TEntity>( new HashSet<TEntity>(entities), dbEntities);

                var parsedEntities = hookContainer.BeforeCreate(diff, context);
                ValidateHookResponse(parsedEntities, actionSource);
                entities = parsedEntities;
            }

            _meta.UpdateMetaInformation(new Type[] { typeof(TEntity) }, ResourceHook.BeforeUpdate);
            BreadthFirstTraverse(entities, (container, entry) =>
            {

                // (var dbEntities, var context) = _meta.GetDatabaseDiff(hookContainer, ResourceHook.BeforeUpdate, relatedEntities);
                object dbEntities = null;
                HookExecutionContext context = null;
                context.Pipeline = actionSource;
                var x = entry.RelationshipGroups;
                var diff = GetEntityDiffInstance(entry.DependentType, entry.UniqueSet, dbEntities)

                return CallHook(container, ResourceHook.BeforeUpdate, new object[] { diff, context });
            });

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
                var parsedEntities = hookContainer.AfterCreate(entities, actionSource);
                ValidateHookResponse(parsedEntities, actionSource);
                entities = parsedEntities;
            }

            _meta.UpdateMetaInformation(new Type[] { typeof(TEntity) }, ResourceHook.AfterUpdate);
            BreadthFirstTraverse(entities, (container, relatedEntities, relationshipGroups) =>
            {
                return CallHook(container, ResourceHook.AfterUpdate, new object[] { relatedEntities, actionSource });
            });

            FlushRegister();
            return entities;
        }

        /// <inheritdoc/>
        public virtual void BeforeRead<TEntity>(ResourceAction actionSource, string stringId = null) where TEntity : class, IIdentifiable
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.BeforeRead);
            hookContainer?.BeforeRead(actionSource, stringId);
            FlushRegister();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> AfterRead<TEntity>(IEnumerable<TEntity> entities, ResourceAction actionSource) where TEntity : class, IIdentifiable
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.AfterRead);
            if (hookContainer != null)
            {
                RegisterProcessedEntities(entities);
                var parsedEntities = hookContainer.AfterRead(entities, actionSource);
                ValidateHookResponse(parsedEntities, actionSource);
                entities = parsedEntities;
            }

            _meta.UpdateMetaInformation(new Type[] { typeof(TEntity) }, new ResourceHook[] { ResourceHook.AfterRead, ResourceHook.BeforeRead });
            BreadthFirstTraverse(entities, (container, relatedEntities, relationshipGroups) =>
            {
                var targetType = TypeHelper.GetListInnerType((IEnumerable)relatedEntities);
                if (_meta.ShouldExecuteHook(targetType, ResourceHook.BeforeRead))
                    CallHook(container, ResourceHook.BeforeRead, new object[] { actionSource, default(string) });

                if (_meta.ShouldExecuteHook(targetType, ResourceHook.AfterRead))
                {
                    return CallHook(container, ResourceHook.AfterRead, new object[] { relatedEntities, actionSource });
                }
                return relatedEntities;
            });

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

                // (var dbEntities, var context) = _meta.GetDatabaseDiff(hookContainer, ResourceHook.BeforeUpdate, entities);
                object dbEntities = null;
                object context = null;
                context.Pipeline = actionSource;
                var diff = new EntityDiff<TEntity>(entities, dbEntities);

                var parsedEntities = hookContainer.BeforeUpdate(diff, context);
                ValidateHookResponse(parsedEntities, actionSource);
                entities = parsedEntities;
            }

            _meta.UpdateMetaInformation(new Type[] { typeof(TEntity) }, ResourceHook.BeforeUpdate);
            BreadthFirstTraverse(entities, (container, relatedEntities, relationshipGroups) =>
            {
                Type innerListType = null;
                Type parameterizedEntityDiff = typeof(EntityDiff<>).MakeGenericType(innerListType);

                // (var dbEntities, var context) = _meta.GetDatabaseDiff(hookContainer, ResourceHook.BeforeUpdate, relatedEntities);
                object dbEntities = null;
                object context = null;
                context.Pipeline = actionSource;
                var diff = Activator.CreateInstance(parameterizedEntityDiff, new object[] { relatedEntities, dbEntities });

                return CallHook(container, ResourceHook.BeforeUpdate, new object[] { diff, context });
            });

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
                var parsedEntities = hookContainer.AfterUpdate(entities, actionSource);
                ValidateHookResponse(parsedEntities, actionSource);
                entities = parsedEntities;
            }

            _meta.UpdateMetaInformation(new Type[] { typeof(TEntity) }, ResourceHook.AfterUpdate);
            BreadthFirstTraverse(entities, (container, relatedEntities, relationshipGroups) =>
            {
                return CallHook(container, ResourceHook.AfterUpdate, new object[] { relatedEntities, actionSource });
            });

            FlushRegister();
            return entities;
        }

        /// <inheritdoc/>
        public virtual void BeforeDelete<TEntity>(IEnumerable<TEntity> entities, ResourceAction actionSource) where TEntity : class, IIdentifiable
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.BeforeDelete);
            hookContainer?.BeforeDelete(entities, actionSource);
            FlushRegister();
        }

        /// <inheritdoc/>
        public virtual void AfterDelete<TEntity>(IEnumerable<TEntity> entities, bool succeeded, ResourceAction actionSource) where TEntity : class, IIdentifiable
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.AfterDelete);
            hookContainer?.AfterDelete(entities, succeeded, actionSource);
            FlushRegister();
        }

        /// <summary>
        /// Fires the hooks for related (nested) entities.
        /// Performs a recursive, forward-looking breadth first traversal 
        /// through the entitis in <paramref name="currentLayer"/> and fires the 
        /// associated resource hooks 
        /// </summary>
        /// <param name="currentLayer">Current layer.</param>
        /// <param name="hookExecutionAction">Hook execution action.</param>
        void BreadthFirstTraverse(
            IEnumerable<IIdentifiable> currentLayer,
            Func<IResourceHookContainer, RelatedEntitiesInCurrentLayerEntry, object> hookExecutionAction
            )
        {
            // for the entities in the current layer: get the collection of all related entities
            var relatedEntitiesInCurrentLayer = ExtractionLoop(currentLayer);

            if (!relatedEntitiesInCurrentLayer.Any()) return;

            // for the unique set of entities in that collection, execute the hooks
            ExecutionLoop(relatedEntitiesInCurrentLayer, hookExecutionAction);
            // for the entities in the current layer: reassign relationships where needed.
            AssignmentLoop(currentLayer, relatedEntitiesInCurrentLayer);

            var nextLayer = relatedEntitiesInCurrentLayer.Values.SelectMany(tuple => tuple.Item2);
            if (nextLayer.Any())
            {
                var uniqueTypesInNextLayer = relatedEntitiesInCurrentLayer.Values.SelectMany(tuple => tuple.Item1.Select(proxy => proxy.TargetType));
                _meta.UpdateMetaInformation(uniqueTypesInNextLayer);
                BreadthFirstTraverse(nextLayer, hookExecutionAction);
            }
        }

        /// <summary>
        /// Iterates through the entities in the current layer. This layer can be inhomogeneous.
        /// For each of these entities: gets all related entity  for which we want to 
        /// execute a hook (target entities), this is defined in MetaInfo.
        /// Grouped per relation, stores these target in relationshipsInCurrentLayer
        /// </summary>
        /// <returns>Hook targets for current layer.</returns>
        /// <param name="currentLayer">Current layer.</param>
        RelatedEntitiesInCurrentLayer ExtractionLoop(
            IEnumerable<IIdentifiable> currentLayer
            )
        {
            var relatedEntitiesInCurrentLayer = new RelatedEntitiesInCurrentLayer();
            foreach (IIdentifiable currentLayerEntity in currentLayer)
            {
                foreach (RelationshipProxy proxy in _meta.GetMetaEntries(currentLayerEntity))
                {
                    var relationshipValue = proxy.GetValue(currentLayerEntity);
                    // skip iteration if there is no relation assigned
                    if (!proxy.IsContextRelation && relationshipValue == null) continue;
                    if (!(relationshipValue is IEnumerable<IIdentifiable> relatedEntities))
                    {
                        // in the case of a to-one relationship, the assigned value
                        // will not be a list. We therefore first wrap it in a list.
                        var list = TypeHelper.CreateListFor(proxy.TargetType);
                        if (relationshipValue != null) list.Add(relationshipValue);
                        relatedEntities = (IEnumerable<IIdentifiable>)list;
                    }
                    // filter the retrieved related entities against 
                    // the entities that were processed in previous iterations,
                    // i.e. against the unique entities in the entire past tree.
                    var newEntitiesInTree = UniqueInTree(relatedEntities, proxy.TargetType);

                    relatedEntitiesInCurrentLayer.Add(relatedEntities, proxy, newEntitiesInTree);
                }
            }
            return relatedEntitiesInCurrentLayer;
        }


        /// <summary>
        /// Executes the hooks for every key in relationshipsInCurrentLayer,
        /// </summary>
        /// <param name="relationshipsInCurrentLayer">Hook targets for current layer.</param>
        /// <param name="hookExecution">Hook execution method.</param>
        void ExecutionLoop(
            RelatedEntitiesInCurrentLayer relatedEntitiesInCurrentLayer,
            Func<IResourceHookContainer, RelatedEntitiesInCurrentLayerEntry, object> hookExecution
            )
        {

            foreach (RelatedEntitiesInCurrentLayerEntry entry in relatedEntitiesInCurrentLayer.Entries())
            {
                var hookContainer = _meta.GetResourceHookContainer(entry.DependentType);
                var castedUniqueSet = TypeHelper.ConvertCollection(entry.UniqueSet, entry.DependentType);
                var filteredUniqueSet = ((IEnumerable)hookExecution(hookContainer, entry)).Cast<IIdentifiable>();
                filteredUniqueSet = new HashSet<IIdentifiable>(filteredUniqueSet);
                entry.UniqueSet.IntersectWith(filteredUniqueSet);
            }
        }

        /// <summary>
        /// When this method is called, the values in relationshipsInCurrentLayer
        /// will contain a subset compared to in the DoExtractionLoop call.
        /// We now need to iterate through currentLayer again and remove any of 
        /// their related entities that do not occur in relationshipsInCurrentLayer
        /// </summary>
        /// <param name="currentLayer">Entities in current layer.</param>
        /// <param name="relationshipsInCurrentLayer">Hook targets for current layer.</param>
        void AssignmentLoop(
            IEnumerable<IIdentifiable> currentLayer,
            Dictionary<Type, (List<RelationshipProxy>, HashSet<IIdentifiable>)> relationshipsInCurrentLayer
            )
        {
            foreach (IIdentifiable currentLayerEntity in currentLayer)
            {
                foreach (RelationshipProxy proxy in _meta.GetMetaEntries(currentLayerEntity))
                {
                    /// if there are no related entities included for 
                    /// currentLayerEntity for this relation, then this key will 
                    /// not exist, and we may continue to the next.
                    if (!relationshipsInCurrentLayer.TryGetValue(proxy.ParentType, out var tuple))
                    {
                        continue;
                    }
                    var parsedEntities = tuple.Item2;

                    var relationshipValue = proxy.GetValue(currentLayerEntity);

                    if (relationshipValue == null)
                    {
                        proxy.SetValue(currentLayerEntity, null);
                    }
                    else if (relationshipValue is IEnumerable<IIdentifiable> relationshipCollection)
                    {
                        var convertedCollection = TypeHelper.ConvertCollection(relationshipCollection.Intersect(parsedEntities), proxy.TargetType);
                        proxy.SetValue(currentLayerEntity, convertedCollection);
                    }
                    else if (relationshipValue is IIdentifiable relationshipSingle)
                    {
                        if (!parsedEntities.Contains(relationshipValue))
                        {
                            proxy.SetValue(currentLayerEntity, null);
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
            if (actionSource != ResourceAction.None && _singleActions.Contains(actionSource) && ((IEnumerable)returnedList).Cast<object>().Count() > 1)
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
        /// TODO:  I tried casting IResourceHookContainer container to type
        /// IResourceHookContainer{IIdentifiable}, which would have allowed us
        /// to call the hook on the nested containers without reflection, but I 
        /// believe this is not possible. We therefore need this helper method.
        /// </summary>
        /// <returns>The hook.</returns>
        /// <param name="container">Container for related entity.</param>
        /// <param name="hook">Target hook type.</param>
        /// <param name="arguments">Arguments to call the hook with.</param>
        object CallHook(IResourceHookContainer container, ResourceHook hook, object[] arguments)
        {
            var method = container.GetType().GetMethod(hook.ToString("G"));
            return method.Invoke(container, arguments);
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

