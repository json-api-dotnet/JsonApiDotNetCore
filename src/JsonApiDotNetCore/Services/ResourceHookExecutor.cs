using System;
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
        private ResourceHook _hookInTreeTraversal;


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
        }


        public virtual IEnumerable<TEntity> BeforeCreate(IEnumerable<TEntity> entities, ResourceAction actionSource)
        {
            var hookContainer = _meta.GetResourceDefinition<TEntity>(ResourceHook.BeforeCreate);
            if (hookContainer == null) return entities;
            /// traversing the 0th layer. Not including this in the recursive function
            /// because the most complexities that arrise in the tree traversal do not
            /// apply to the 0th layer (eg non-homogeneity of the next layers)
            entities = hookContainer.BeforeCreate(entities, actionSource); // eg all of type {Article}

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

            return entities;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> AfterCreate(IEnumerable<TEntity> entities, ResourceAction actionSource)
        {
            var hookContainer = _meta.GetResourceDefinition<TEntity>(ResourceHook.AfterCreate);
            if (hookContainer == null) return entities;

            return hookContainer.AfterCreate(entities, actionSource);
        }

        /// <inheritdoc/>
        public virtual void BeforeRead(ResourceAction actionSource, string stringId = null)
        {
            var hookContainer = _meta.GetResourceDefinition<TEntity>(ResourceHook.BeforeRead);
            if (hookContainer == null) return;


            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> AfterRead(IEnumerable<TEntity> entities, ResourceAction actionSource)
        {
            var hookContainer = _meta.GetResourceDefinition<TEntity>(ResourceHook.AfterRead);
            if (hookContainer == null) return entities;


            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> BeforeUpdate(IEnumerable<TEntity> entities, ResourceAction actionSource)
        {
            var hookContainer = _meta.GetResourceDefinition<TEntity>(ResourceHook.BeforeUpdate);
            if (hookContainer == null) return entities;


            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> AfterUpdate(IEnumerable<TEntity> entities, ResourceAction actionSource)
        {
            var hookContainer = _meta.GetResourceDefinition<TEntity>(ResourceHook.AfterUpdate);
            if (hookContainer == null) return entities;


            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual void BeforeDelete(IEnumerable<TEntity> entities, ResourceAction actionSource)
        {
            var hookContainer = _meta.GetResourceDefinition<TEntity>(ResourceHook.BeforeDelete);
            if (hookContainer == null) return;

            hookContainer.BeforeDelete(entities, actionSource);
        }

        /// <inheritdoc/>
        public virtual void AfterDelete(IEnumerable<TEntity> entities, bool succeeded, ResourceAction actionSource)
        {
            var hookContainer = _meta.GetResourceDefinition<TEntity>(ResourceHook.AfterDelete);
            if (hookContainer == null) return;

            hookContainer.AfterDelete(entities, succeeded, actionSource);
        }

        void BreadthFirstTraverse(
            IEnumerable<IIdentifiable> currentLayer,
            Func<IResourceHookContainer<IIdentifiable>, IEnumerable<IIdentifiable>, IEnumerable<IIdentifiable>> hookExecutionAction
            )
        {
            // for the entities in the current layer: get the collection of all related entities
            var relationshipsInCurrentLayer = ExtractionLoop(currentLayer);
            // for the unique set of entities in that collection, execute the hooks
            ExecutionLoop(relationshipsInCurrentLayer, hookExecutionAction);
            // for the entities in the current layer: reassign relationships where needed.
            AssignmentLoop(currentLayer, relationshipsInCurrentLayer);

            var nextLayer = relationshipsInCurrentLayer.Values.SelectMany(entities => entities);
            if (nextLayer.Any())
            {
                var uniqueTypesInNextLayer = relationshipsInCurrentLayer.Keys.Select(k => k.Type);
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
        Dictionary<RelationshipAttribute, IEnumerable<IIdentifiable>> ExtractionLoop(
            IEnumerable<IIdentifiable> currentLayer
            )
        {
            var relationshipsInCurrentLayer = new Dictionary<RelationshipAttribute, IEnumerable<IIdentifiable>>();
            foreach (IIdentifiable currentLayerEntity in currentLayer)
            {
                foreach (RelationshipAttribute attr in _meta.GetMetaEntries(currentLayerEntity))
                {
                    var relationshipValue = attr.GetValue(currentLayerEntity);
                    if (!(relationshipValue is IEnumerable<IIdentifiable>))
                    {
                        // in the case of a to-one relationship, the assigned value
                        // will not be a list. We therefore first wrap it in a list.
                        var list = TypeHelper.CreateListFor(relationshipValue.GetType());
                        list.Add(relationshipValue);
                        relationshipValue = list;
                    }
                    var relatedEntities = relationshipValue as IEnumerable<IIdentifiable>;
                    if (!relationshipsInCurrentLayer.ContainsKey(attr))
                    {
                        relationshipsInCurrentLayer[attr] = relatedEntities;
                    }
                    else
                    {
                        relationshipsInCurrentLayer[attr].Concat(relatedEntities);
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
            Dictionary<RelationshipAttribute, IEnumerable<IIdentifiable>> relationshipsInCurrentLayer,
            Func<IResourceHookContainer<IIdentifiable>, IEnumerable<IIdentifiable>, IEnumerable<IIdentifiable>> hookExecution
            )
        {
            foreach (var pair in relationshipsInCurrentLayer)
            {
                var attr = pair.Key;
                var uniqueEntities = new HashSet<IIdentifiable>(pair.Value);
                var executor = _meta.GetResourceDefinition(attr.Type);
                var filteredUniqueEntites = hookExecution(executor, uniqueEntities);
                relationshipsInCurrentLayer[pair.Key] = filteredUniqueEntites.ToArray();
            }
        }

        /// <summary>
        /// When this method is called, the values in relationshipsInCurrentLayer
        /// will contain a subset compared to in the DoExtractionLoop call.
        /// We now need to iterate through currentLayer again and remove any of 
        /// their related entities that do not occur in relationshipsInCurrentLayer
        /// </summary>
        /// <param name="currentLayer">Current layer.</param>
        /// <param name="relationshipsInCurrentLayer">Hook targets for current layer.</param>
        void AssignmentLoop(
            IEnumerable<IIdentifiable> currentLayer,
            Dictionary<RelationshipAttribute, IEnumerable<IIdentifiable>> relationshipsInCurrentLayer
            )
        {
            // @TODO IM NOT EVEN SURE IF WE NEED TO REASSIGN ?! 
            // if we adjust same objects in memory, we should only be required 
            // to perform filter check
            foreach (IIdentifiable currentLayerEntity in currentLayer)
            {
                foreach (RelationshipAttribute attr in _meta.GetMetaEntries(currentLayerEntity))
                {
                    var parsedEntities = relationshipsInCurrentLayer[attr];

                    var relationshipValue = attr.GetValue(currentLayerEntity);
                    if (relationshipValue is IEnumerable<IIdentifiable> relationshipCollection)
                    {
                        relationshipValue = relationshipCollection.Intersect(parsedEntities);
                        attr.SetValue(currentLayerEntity, relationshipValue);
                    }
                    else if (relationshipValue is IIdentifiable relationshipSingle)
                    {
                        if (!parsedEntities.Contains(relationshipValue))
                        {
                            attr.SetValue(currentLayerEntity, null); 
                    }
                }
            }
        }
    }
}
