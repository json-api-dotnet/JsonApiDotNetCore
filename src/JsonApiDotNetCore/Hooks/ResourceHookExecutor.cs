using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using PrincipalType = System.Type;


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
        protected readonly EntityTreeLayerFactory _layerFactory;
        protected readonly IHookExecutorHelper _meta;
        protected readonly IJsonApiContext _context;
        private readonly IResourceGraph _graph;
        protected Dictionary<Type, HashSet<IIdentifiable>> _processedEntities;


        public ResourceHookExecutor(
            IHookExecutorHelper meta,
            IJsonApiContext context,
            IResourceGraph graph
            )
        {
            _meta = meta;
            _context = context;
            _graph = graph;
            _processedEntities = new Dictionary<Type, HashSet<IIdentifiable>>();
            _layerFactory = new EntityTreeLayerFactory(meta, graph, _processedEntities);
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> BeforeCreate<TEntity>(IEnumerable<TEntity> entities, ResourceAction actionSource) where TEntity : class, IIdentifiable
        {
            return entities;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> AfterCreate<TEntity>(IEnumerable<TEntity> entities, ResourceAction actionSource) where TEntity : class, IIdentifiable
        {
            return entities;
        }

        /// <inheritdoc/>
        public virtual void BeforeRead<TEntity>(ResourceAction actionSource, string stringId = null) where TEntity : class, IIdentifiable
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.BeforeRead);
            hookContainer?.BeforeRead(actionSource, false, stringId);

            var contextEntity = _graph.GetContextEntity(typeof(TEntity));
            var calledContainers = new List<Type>() { typeof(TEntity) };
            foreach (var relationshipPath in _context.IncludedRelationships)
            {
                // TODO: Get rid of nested boolean and calledContainers, add BeforeReadRelation hook
                RecursiveBeforeRead(contextEntity, relationshipPath.Split('.').ToList(), actionSource, calledContainers);
            }


        }

        void RecursiveBeforeRead(ContextEntity contextEntity, List<string> relationshipChain, ResourceAction actionSource, List<Type> calledContainers)
        {
            var target = relationshipChain.First();
            var relationship = contextEntity.Relationships.FirstOrDefault(r => r.PublicRelationshipName == target);
            if (relationship == null)
            {
                throw new JsonApiException(400, $"Invalid relationship {target} on {contextEntity.EntityName}",
                    $"{contextEntity.EntityName} does not have a relationship named {target}");
            }

            if (!calledContainers.Contains(relationship.Type))
            {
                calledContainers.Add(relationship.Type);
                var container = _meta.GetResourceHookContainer(relationship.Type, ResourceHook.BeforeRead);
                if (container != null)
                {
                    CallHook(container, ResourceHook.BeforeRead, new object[] { actionSource, true, null });
                }
            }
            relationshipChain.RemoveAt(0);
            if (relationshipChain.Any())
            {

                RecursiveBeforeRead(_graph.GetContextEntity(relationship.Type), relationshipChain, actionSource, calledContainers);
            }

        }


        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> AfterRead<TEntity>(IEnumerable<TEntity> entities, ResourceAction pipeline) where TEntity : class, IIdentifiable
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.AfterRead);
            var layer = _layerFactory.CreateLayer(entities);
            var uniqueEntities = layer.GetAllUniqueEntities().Cast<TEntity>();
            if (hookContainer != null)
            {
                var filteredUniqueEntities = hookContainer?.AfterRead(uniqueEntities, pipeline, false);
                entities = entities.Intersect(filteredUniqueEntities);
            }
            var nextLayer = _layerFactory.CreateLayer(layer);
            RecursiveAfterRead(nextLayer, pipeline);
            FlushRegister();
            return entities;
        }

        void RecursiveAfterRead(EntityTreeLayer currentLayer, ResourceAction pipeline)
        {
            foreach (NodeInLayer node in currentLayer)
            {
                var entityType = node.EntityType;
                var hookContainer = _meta.GetResourceHookContainer(entityType, ResourceHook.AfterRead);
                if (hookContainer == null) continue;

                var filteredUniqueSet = CallHook(hookContainer, ResourceHook.AfterRead, new object[] { node.UniqueSet, pipeline, true }).Cast<IIdentifiable>();

                var hashSetUnique = new HashSet<IIdentifiable>(filteredUniqueSet);
                foreach (var originRelationship in node.OriginEntities)
                {
                    var proxy = originRelationship.Key;
                    var previousEntities = originRelationship.Value;
                    foreach (var prevEntity in previousEntities)
                    {
                        var actualValue = proxy.GetValue(prevEntity);

                        if (actualValue is IEnumerable<IIdentifiable> relationshipCollection)
                        {
                            var convertedCollection = TypeHelper.ConvertCollection(relationshipCollection.Intersect(hashSetUnique), entityType);
                            proxy.SetValue(prevEntity, convertedCollection);
                        }
                        else if (actualValue is IIdentifiable relationshipSingle)
                        {
                            if (!hashSetUnique.Contains(actualValue))
                            {
                                proxy.SetValue(prevEntity, null);
                            }
                        }
                    }
                }

            }
            EntityTreeLayer nextLayer = _layerFactory.CreateLayer(currentLayer);
            if (nextLayer.Any()) RecursiveAfterRead(nextLayer, pipeline);
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> BeforeUpdate<TEntity>(IEnumerable<TEntity> entities, ResourceAction actionSource) where TEntity : class, IIdentifiable
        {
            FlushRegister();
            return entities;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> AfterUpdate<TEntity>(IEnumerable<TEntity> entities, ResourceAction actionSource) where TEntity : class, IIdentifiable
        {
            return entities;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> BeforeDelete<TEntity>(IEnumerable<TEntity> entities, ResourceAction actionSource) where TEntity : class, IIdentifiable
        {
            return entities;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> AfterDelete<TEntity>(IEnumerable<TEntity> entities, ResourceAction actionSource, bool succeeded) where TEntity : class, IIdentifiable
        {

            return entities;
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
            return (IEnumerable)ThrowJsonApiExceptionOnError(() => method.Invoke(container, arguments));
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
    }
}

