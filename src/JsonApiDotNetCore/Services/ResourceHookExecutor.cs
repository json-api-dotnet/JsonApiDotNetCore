using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Models;


namespace JsonApiDotNetCore.Services
{


    /// <inheritdoc/>
    public class ResourceHookExecutor<TEntity> : IResourceHookExecutor<TEntity> where TEntity : class, IIdentifiable
    {
        protected readonly ResourceHook[] _implementedHooks;
        protected readonly IJsonApiContext _jsonApiContext;
        protected readonly IGenericProcessorFactory _genericProcessorFactory;
        protected readonly ResourceDefinition<TEntity> _resourceDefinition;
        protected readonly IResourceGraph _graph;
        protected readonly Type _entityType;
        protected readonly IResourceHookMetaInfo _meta;
        protected readonly ResourceAction[] _singleActions;
        private ResourceHook _hookInTreeTraversal;
        protected Dictionary<Type, HashSet<IIdentifiable>> _processedEntities;



        public ResourceHookExecutor(
            IJsonApiContext jsonApiContext,
            IImplementedResourceHooks<TEntity> hooksConfiguration,
            IResourceHookMetaInfo meta
            )
        {
            _genericProcessorFactory = jsonApiContext.GenericProcessorFactory;
            _jsonApiContext = jsonApiContext;
            _graph = _jsonApiContext.ResourceGraph;
            _meta = meta;
            _implementedHooks = hooksConfiguration.ImplementedHooks;
            _entityType = typeof(TEntity);
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

        public virtual IEnumerable<TEntity> BeforeCreate(IEnumerable<TEntity> entities, ResourceAction actionSource)
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.BeforeCreate);

            /// traversing the 0th layer. Not including this in the recursive function
            /// because the most complexities that arrise in the tree traversal do not
            /// apply to the 0th layer (eg non-homogeneity of the next layers)
            if (hookContainer != null)
            {
                RegisterProcessedEntities(entities);
                var parsedEntities = hookContainer.BeforeCreate(entities, actionSource); // eg all of type {Article}
                ValidateHookResponse(entities, parsedEntities, actionSource);
                entities = parsedEntities;
            }


            /// We use IIdentifiable instead of TEntity, because deeper layers
            /// in the tree traversal will not necessarily be homogenous (i.e. 
            /// not all elements will be some same type T).
            /// eg: this list will be all of type {Article}, but deeper layers 
            /// could consist of { Tag, Author, Comment }
            _meta.UpdateMetaInformation(new Type[] { _entityType }, ResourceHook.BeforeUpdate);
            BreadthFirstTraverse(entities, (container, relatedEntities) =>
            {
                return container.BeforeUpdate(relatedEntities, actionSource);
            });

            FlushRegister();
            return entities;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> AfterCreate(IEnumerable<TEntity> entities, ResourceAction actionSource)
        {
            RegisterProcessedEntities(entities);
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.AfterCreate);
            /// @TODO: even if we don't have an implementation for eg TodoItem AfterCreate, 
            /// we should still consider to fire the hooks of its relation, eg TodoItem.Owner

            _meta.UpdateMetaInformation(new Type[] { _entityType }, ResourceHook.AfterUpdate);
            BreadthFirstTraverse(entities, (container, relatedEntities) =>
            {
                return container.AfterUpdate(relatedEntities, actionSource);
            });

            if (hookContainer != null)
            {
                var parsedEntities = hookContainer.AfterCreate(entities, actionSource);
                ValidateHookResponse(entities, parsedEntities, actionSource);
                return parsedEntities;
            }

            FlushRegister();
            return entities;
        }

        /// <inheritdoc/>
        public virtual void BeforeRead(ResourceAction actionSource, string stringId = null)
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.BeforeRead);
            if (hookContainer == null) return;
            hookContainer.BeforeRead(actionSource, stringId);
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> AfterRead(IEnumerable<TEntity> entities, ResourceAction actionSource)
        {
            RegisterProcessedEntities(entities);
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.AfterRead);

            _meta.UpdateMetaInformation(new Type[] { _entityType }, new ResourceHook[] { ResourceHook.AfterRead, ResourceHook.BeforeRead });
            BreadthFirstTraverse(entities, (container, relatedEntities) =>
            {
                if (container.ShouldExecuteHook(ResourceHook.BeforeRead)) container.BeforeRead(actionSource);
                if (container.ShouldExecuteHook(ResourceHook.AfterRead))
                {
                    var parsedEntities = container.AfterRead(relatedEntities, actionSource);
                    ValidateHookResponse(relatedEntities, parsedEntities);
                    return parsedEntities;
                }
                return relatedEntities;
            });

            if (hookContainer != null)
            {
                var parsedEntities = hookContainer.AfterRead(entities, actionSource);
                ValidateHookResponse(entities, parsedEntities, actionSource);
                return parsedEntities;
            }

            FlushRegister();
            return entities;
        }
        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> BeforeUpdate(IEnumerable<TEntity> entities, ResourceAction actionSource)
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.BeforeUpdate);
            if (hookContainer != null)
            {
                RegisterProcessedEntities(entities);
                var parsedEntities = hookContainer.BeforeUpdate(entities, actionSource);
                ValidateHookResponse(entities, parsedEntities, actionSource);
                entities = parsedEntities;
            }

            _meta.UpdateMetaInformation(new Type[] { _entityType }, ResourceHook.BeforeUpdate);
            BreadthFirstTraverse(entities, (container, relatedEntities) =>
            {
                return container.BeforeUpdate(relatedEntities, actionSource);
            });

            FlushRegister();
            return entities;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> AfterUpdate(IEnumerable<TEntity> entities, ResourceAction actionSource)
        {
            RegisterProcessedEntities(entities);
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.AfterUpdate);

            _meta.UpdateMetaInformation(new Type[] { _entityType }, ResourceHook.AfterUpdate);
            BreadthFirstTraverse(entities, (container, relatedEntities) =>
            {
                return container.AfterUpdate(relatedEntities, actionSource);
            });

            if (hookContainer != null)
            {
                var parsedEntities = hookContainer.AfterUpdate(entities, actionSource);
                ValidateHookResponse(entities, parsedEntities, actionSource);
                return parsedEntities;
            }

            FlushRegister();
            return entities;
        }

        /// <inheritdoc/>
        public virtual void BeforeDelete(IEnumerable<TEntity> entities, ResourceAction actionSource)
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.BeforeDelete);
            if (hookContainer == null) return;
            hookContainer.BeforeDelete(entities, actionSource);
        }

        /// <inheritdoc/>
        public virtual void AfterDelete(IEnumerable<TEntity> entities, bool succeeded, ResourceAction actionSource)
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.AfterDelete);
            if (hookContainer == null) return;
            hookContainer.AfterDelete(entities, succeeded, actionSource);
        }

        /// <summary>
        /// Fires the hooks for related (nested) entities.
        /// </summary>
        /// <param name="currentLayer">Current layer.</param>
        /// <param name="hookExecutionAction">Hook execution action.</param>
        void BreadthFirstTraverse(
            IEnumerable<IIdentifiable> currentLayer,
            Func<IResourceHookContainer<IIdentifiable>, IEnumerable<IIdentifiable>, IEnumerable<IIdentifiable>> hookExecutionAction
            )
        {
            // for the entities in the current layer: get the collection of all related entities
            var relationshipsInCurrentLayer = ExtractionLoop(currentLayer);

            if (!relationshipsInCurrentLayer.Any()) return;

            // for the unique set of entities in that collection, execute the hooks
            ExecutionLoop(relationshipsInCurrentLayer, hookExecutionAction);
            // for the entities in the current layer: reassign relationships where needed.
            AssignmentLoop(currentLayer, relationshipsInCurrentLayer);

            var nextLayer = relationshipsInCurrentLayer.Values.SelectMany(tuple => tuple.Item2);
            if (nextLayer.Any())
            {
                var uniqueTypesInNextLayer = relationshipsInCurrentLayer.Values.SelectMany(tuple => tuple.Item1.Select(proxy => proxy.TargetType));
                _meta.UpdateMetaInformation(uniqueTypesInNextLayer);
                BreadthFirstTraverse(nextLayer, hookExecutionAction);
            }
        }

        /// <summary>
        ///     * Iterates through the entities in the current. This layer can be inhomogeneous.
        ///     * For each of these entities: gets all related entity  for which we want to 
        ///       execute a hook (target entities), this is defined in MetaInfo.
        ///     * Grouped per relation, stores these target in relationshipsInCurrentLayer
        /// </summary>
        /// <returns>Hook targets for current layer.</returns>
        /// <param name="currentLayer">Current layer.</param>
        Dictionary<Type, (List<RelationshipProxy>, HashSet<IIdentifiable>)> ExtractionLoop(
            IEnumerable<IIdentifiable> currentLayer
            )
        {
            var relationshipsInCurrentLayer = new Dictionary<Type, (List<RelationshipProxy>, HashSet<IIdentifiable>)>();
            foreach (IIdentifiable currentLayerEntity in currentLayer)
            {
                foreach (RelationshipProxy proxy in _meta.GetMetaEntries(currentLayerEntity))
                {
                    var relationshipValue = proxy.GetValue(currentLayerEntity);
                    // skip iteration if there is no relation assigned
                    if (relationshipValue == null) continue;
                    if (!(relationshipValue is IEnumerable<IIdentifiable> relatedEntities))
                    {
                        // in the case of a to-one relationship, the assigned value
                        // will not be a list. We therefore first wrap it in a list.
                        var list = TypeHelper.CreateListFor(relationshipValue.GetType());
                        list.Add(relationshipValue);
                        relatedEntities = (IEnumerable<IIdentifiable>)list;
                    }

                    // filter the retrieved related entities collection against the entities that were processed in previous iterations
                    var newEntitiesInTree = UniqueInTree(relatedEntities, proxy.TargetType);
                    if (!newEntitiesInTree.Any()) continue;
                    if (!relationshipsInCurrentLayer.ContainsKey(proxy.ParentType))
                    {
                        relationshipsInCurrentLayer[proxy.ParentType] = (new List<RelationshipProxy>() { proxy }, newEntitiesInTree);
                    }
                    else
                    {
                        (var proxies, var entities) = relationshipsInCurrentLayer[proxy.ParentType];
                        entities.UnionWith(newEntitiesInTree);
                        if (!proxies.Select(p => p.RelationshipIdentifier).Contains(proxy.RelationshipIdentifier)) proxies.Add(proxy);
                    }
                }
            }
            return relationshipsInCurrentLayer;
        }


        /// <summary>
        /// Executes the hooks for every key in relationshipsInCurrentLayer,
        /// </summary>
        /// <param name="relationshipsInCurrentLayer">Hook targets for current layer.</param>
        /// <param name="hookExecution">Hook execution.</param>
        void ExecutionLoop(
            Dictionary<Type, (List<RelationshipProxy>, HashSet<IIdentifiable>)> relationshipsInCurrentLayer,
            Func<IResourceHookContainer<IIdentifiable>, IEnumerable<IIdentifiable>, IEnumerable<IIdentifiable>> hookExecution
            )
        {
            // note that it is possible that we have multiple relations to one type.
            var parentTypes = relationshipsInCurrentLayer.Keys.ToArray();

            foreach (Type type in parentTypes)
            {
                (var relationshipsProxy, var relatedEntities) = relationshipsInCurrentLayer[type];
                var hookContainer = _meta.GetResourceHookContainer(relationshipsProxy.First().TargetType);
                var filteredEntities = new HashSet<IIdentifiable>(hookExecution(hookContainer, relatedEntities));
                relationshipsInCurrentLayer[type] = (relationshipsProxy, filteredEntities);
            }
        }

        /// <summary>
        /// B this method is called, the values in relationshipsInCurrentLayer
        /// will contain a subset compared to in the DoExtractionLoop call.
        /// We now need to iterate through currentLayer again and remove any of 
        /// their related entities that do not occur in relationshipsInCurrentLayer
        /// </summary>
        /// <param name="currentLayer">Current layer.</param>
        /// <param name="relationshipsInCurrentLayer">Hook targets for current layer.</param>
        void AssignmentLoop(
            IEnumerable<IIdentifiable> currentLayer,
            Dictionary<Type, (List<RelationshipProxy>, HashSet<IIdentifiable>)> relationshipsInCurrentLayer
            )
        {
            // @TODO IM NOT EVEN SURE IF WE NEED TO REASSIGN ?! 
            // if we adjust same objects in memory, we should only be required 
            // to perform filter check
            foreach (IIdentifiable currentLayerEntity in currentLayer)
            {
                foreach (RelationshipProxy proxy in _meta.GetMetaEntries(currentLayerEntity))
                {

                    /// if there are no related entities included for 
                    /// currentLayerEntity for this relation, then this key will 
                    /// not exist, and we may continue to the next.
                    if (!relationshipsInCurrentLayer.TryGetValue(proxy.TargetType, out var tuple))
                    {
                        continue;
                    }
                    var parsedEntities = tuple.Item2;

                    var relationshipValue = proxy.GetValue(currentLayerEntity);
                    if (relationshipValue is IEnumerable<IIdentifiable> relationshipCollection)
                    {
                        relationshipCollection = (relationshipCollection.Intersect(parsedEntities));
                        proxy.SetValue(currentLayerEntity, relationshipCollection);
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
        /// Ensures that the return type from the hook matches the required type.
        /// And when relevant, eg. AfterRead when fired from GetAsync(TId id), checks that the collection
        /// does not contain more than one item.
        /// </summary>
        /// <param name="initalList">The initial collection before the hook was executed.</param>
        /// <param name="returnedList"> The collection returned from the hook</param>
        /// <param name="actionSource">The pipeine from which the hook was fired</param>
        protected void ValidateHookResponse(IEnumerable<IIdentifiable> initalList, IEnumerable<IIdentifiable> returnedList, ResourceAction actionSource = 0)
        {
            if (TypeHelper.GetListInnerType(initalList as IEnumerable) != TypeHelper.GetListInnerType(returnedList as IEnumerable))
            {
                throw new ApplicationException("The List type of the return value from a resource hook" +
                    "did not match the the same type as the original collection. Make sure you are returning collections of the same" +
                    "entities as recieved from your hooks");
            }
            if (actionSource != ResourceAction.None && _singleActions.Contains(actionSource) && returnedList.Count() > 1)
            {
                throw new ApplicationException("The returned collection from this hook may only contain one item in the case of he" +
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
        void RegisterProcessedEntities(IEnumerable<TEntity> entities)
        {
            RegisterProcessedEntities(entities, _entityType);
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

        void FlushRegister()
        {
            _processedEntities = new Dictionary<Type, HashSet<IIdentifiable>>();
        }
    }
}

