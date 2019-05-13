using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using PrincipalType = System.Type;
using DependentType = System.Type;


namespace JsonApiDotNetCore.Services
{
    /// <inheritdoc/>
    public class ResourceHookExecutor : IResourceHookExecutor
    {
        public static readonly IdentifiableComparer Comparer = new IdentifiableComparer();
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
            _layerFactory = new EntityTreeLayerFactory(meta, graph, _context, _processedEntities);
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> BeforeCreate<TEntity>(IEnumerable<TEntity> entities, ResourceAction pipeline) where TEntity : class, IIdentifiable
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.BeforeCreate);
            var layer = _layerFactory.CreateLayer(entities);
            if (hookContainer != null)
            {
                var uniqueEntities = layer.GetAllUniqueEntities().Cast<TEntity>();
                IEnumerable<TEntity> filteredUniqueEntities = hookContainer.BeforeCreate(uniqueEntities, pipeline);
                entities = entities.Intersect(filteredUniqueEntities, Comparer).Cast<TEntity>().ToList();
            }
            EntityTreeLayer nextLayer = _layerFactory.CreateLayer(layer);

            BeforeUpdateRelationship(pipeline, nextLayer);
            FlushRegister();
            return entities;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> AfterCreate<TEntity>(IEnumerable<TEntity> entities, ResourceAction pipeline) where TEntity : class, IIdentifiable
        {
            return entities;
        }

        /// <inheritdoc/>
        public virtual void BeforeRead<TEntity>(ResourceAction pipeline, string stringId = null) where TEntity : class, IIdentifiable
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.BeforeRead);
            hookContainer?.BeforeRead(pipeline, false, stringId);

            var contextEntity = _graph.GetContextEntity(typeof(TEntity));
            var calledContainers = new List<Type>() { typeof(TEntity) };
            foreach (var relationshipPath in _context.IncludedRelationships)
            {
                // TODO: Get rid of nested boolean and calledContainers, add BeforeReadRelation hook
                RecursiveBeforeRead(contextEntity, relationshipPath.Split('.').ToList(), pipeline, calledContainers);
            }


        }

        void RecursiveBeforeRead(ContextEntity contextEntity, List<string> relationshipChain, ResourceAction pipeline, List<Type> calledContainers)
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
                    CallHook(container, ResourceHook.BeforeRead, new object[] { pipeline, true, null });
                }
            }
            relationshipChain.RemoveAt(0);
            if (relationshipChain.Any())
            {

                RecursiveBeforeRead(_graph.GetContextEntity(relationship.Type), relationshipChain, pipeline, calledContainers);
            }

        }


        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> AfterRead<TEntity>(IEnumerable<TEntity> entities, ResourceAction pipeline) where TEntity : class, IIdentifiable
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.AfterRead);
            var layer = _layerFactory.CreateLayer(entities);
            if (hookContainer != null)
            {
                var uniqueEntities = layer.GetAllUniqueEntities().Cast<TEntity>();
                var filteredUniqueEntities = hookContainer?.AfterRead(uniqueEntities, pipeline, false);
                entities = entities.Intersect(filteredUniqueEntities, Comparer).Cast<TEntity>();
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
                node.UpdateUniqueSet(filteredUniqueSet);
                Reassign(node);

            }
            EntityTreeLayer nextLayer = _layerFactory.CreateLayer(currentLayer);
            if (nextLayer.Any()) RecursiveAfterRead(nextLayer, pipeline);
        }

        private void Reassign(NodeInLayer node)
        {
            var updatedUniqueSet = node.UniqueSet.Cast<IIdentifiable>().ToList();
            var principalType = node.EntityType;
            foreach (var originRelationship in node.PrincipalEntitiesByRelationships)
            {
                var proxy = originRelationship.Key;
                var previousEntities = originRelationship.Value;
                foreach (var prevEntity in previousEntities)
                {
                    var actualValue = proxy.GetValue(prevEntity);

                    if (actualValue is IEnumerable<IIdentifiable> relationshipCollection)
                    {
                        var convertedCollection = TypeHelper.ConvertCollection(relationshipCollection.Intersect(updatedUniqueSet, Comparer), principalType);
                        proxy.SetValue(prevEntity, convertedCollection);
                    }
                    else if (actualValue is IIdentifiable relationshipSingle)
                    {
                        if (!updatedUniqueSet.Intersect(new HashSet<IIdentifiable>() { relationshipSingle }, Comparer).Any())
                        {
                            proxy.SetValue(prevEntity, null);
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> BeforeUpdate<TEntity>(IEnumerable<TEntity> entities, ResourceAction pipeline) where TEntity : class, IIdentifiable
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.BeforeUpdate);
            var layer = _layerFactory.CreateLayer(entities);
            if (hookContainer != null)
            {
                List<RelationshipProxy> relationships = layer.GetRelationships(typeof(TEntity));
                List<TEntity> uniqueEntities = layer.GetAllUniqueEntities().Cast<TEntity>().ToList();
                IEnumerable<TEntity> dbValues = (IEnumerable<TEntity>)LoadDbValues(uniqueEntities, relationships, typeof(TEntity), ResourceHook.BeforeUpdate);
                var diff = new EntityDiff<TEntity>(uniqueEntities, dbValues);
                IEnumerable<TEntity> filteredUniqueEntities = hookContainer.BeforeUpdate(diff, pipeline);
                entities = entities.Intersect(filteredUniqueEntities, Comparer).Cast<TEntity>().ToList();
            }
            EntityTreeLayer nextLayer = _layerFactory.CreateLayer(layer);
            BeforeUpdateRelationship(pipeline, nextLayer);
            FlushRegister();
            return entities;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> BeforeDelete<TEntity>(IEnumerable<TEntity> entities, ResourceAction pipeline) where TEntity : class, IIdentifiable
        {
            var hookContainer = _meta.GetResourceHookContainer<TEntity>(ResourceHook.BeforeDelete);
            var layer = _layerFactory.CreateLayer(entities);
            List<RelationshipProxy> relationships = layer.GetRelationships(typeof(TEntity));
            if (hookContainer != null)
            {
                List<TEntity> uniqueEntities = layer.GetAllUniqueEntities().Cast<TEntity>().ToList();
                IEnumerable<TEntity> filteredUniqueEntities = hookContainer.BeforeDelete(uniqueEntities, pipeline);
                entities = entities.Intersect(filteredUniqueEntities, Comparer).Cast<TEntity>().ToList();
            }

            var relationshipsByDependentType = relationships.GroupBy(proxy => proxy.DependentType).ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());

            foreach (var group in relationshipsByDependentType)
            {
                var dependentType = group.Key;
                var nestedHookcontainer = _meta.GetResourceHookContainer(dependentType, ResourceHook.BeforeImplicitUpdateRelationship);
                if (nestedHookcontainer == null) continue;
                var implicitlyAffectedRelationships = new Dictionary<RelationshipProxy, List<IIdentifiable>>();
                var castedEntities = entities.Cast<IIdentifiable>().ToList();
                group.Value.ForEach(proxy => implicitlyAffectedRelationships.Add(proxy, castedEntities));
                Dictionary<RelationshipProxy, List<IIdentifiable>> implicitlyAffectedDependents = LoadImplicitlyAffected(implicitlyAffectedRelationships);
                var relationshipHelper = TypeHelper.CreateInstanceOfOpenType(typeof(UpdatedRelationshipHelper<>), dependentType, implicitlyAffectedDependents);
                CallHook(nestedHookcontainer, ResourceHook.BeforeImplicitUpdateRelationship, new object[] { relationshipHelper, pipeline, });
            }

            FlushRegister();
            return entities;

        }


        private void BeforeUpdateRelationship(ResourceAction pipeline, EntityTreeLayer layer)
        {
            foreach (NodeInLayer node in layer)
            {
                var nestedHookcontainer = _meta.GetResourceHookContainer(node.EntityType, ResourceHook.BeforeUpdateRelationship);
                if (nestedHookcontainer != null)
                {
                    IEnumerable<IIdentifiable> uniqueEntities = node.UniqueSet.Cast<IIdentifiable>();
                    if (uniqueEntities.Any())
                    {
                        var uniqueIds = uniqueEntities.Select(e => e.StringId);
                        IEnumerable<IIdentifiable> dbValues = LoadDbValues(node.UniqueSet, node.Relationships, node.EntityType, ResourceHook.BeforeUpdateRelationship)?.Cast<IIdentifiable>();
                        Dictionary<RelationshipProxy, List<IIdentifiable>> dependentsByRelationships = node.EntitiesByRelationships;

                        var relationshipHelper = TypeHelper.CreateInstanceOfOpenType(typeof(UpdatedRelationshipHelper<>), node.EntityType, dependentsByRelationships);
                        var allowedIds = CallHook(nestedHookcontainer, ResourceHook.BeforeUpdateRelationship, new object[] { uniqueIds, relationshipHelper, pipeline }).Cast<string>();
                        var allowedUniqueEntities = uniqueEntities.Where(ue => allowedIds.Contains(ue.StringId));
                        node.UpdateUniqueSet(allowedUniqueEntities);
                        Reassign(node);
                    }
                }
                nestedHookcontainer = _meta.GetResourceHookContainer(node.EntityType, ResourceHook.BeforeImplicitUpdateRelationship);
                if (nestedHookcontainer != null)
                {
                    var uniqueEntities = node.UniqueSet.Cast<IIdentifiable>().ToList();
                    var entityType = node.EntityType;
                    Dictionary<RelationshipProxy, List<IIdentifiable>> relationships = node.PrincipalEntitiesByRelationships;
                    Dictionary<RelationshipProxy, List<IIdentifiable>> implicitlyAffectedDependents = LoadImplicitlyAffected(relationships, uniqueEntities);
                    if (implicitlyAffectedDependents.Any())
                    {
                        var relationshipHelper = TypeHelper.CreateInstanceOfOpenType(typeof(UpdatedRelationshipHelper<>), node.EntityType, implicitlyAffectedDependents);
                        CallHook(nestedHookcontainer, ResourceHook.BeforeImplicitUpdateRelationship, new object[] { relationshipHelper, pipeline, });
                    }
                }


                if (node.EntitiesByRelationships.Any())
                {
                    var entityType = node.EntitiesByRelationships.First().Key.PrincipalType;
                    nestedHookcontainer = _meta.GetResourceHookContainer(entityType, ResourceHook.BeforeImplicitUpdateRelationship);
                    if (nestedHookcontainer != null)
                    {
                        var inverseRelationships = node.EntitiesByRelationships.ToDictionary(kvp => GetInverseRelationship(kvp.Key), kvp => kvp.Value);
                        Dictionary<RelationshipProxy, List<IIdentifiable>> implicitlyAffectedDependents = LoadImplicitlyAffected(inverseRelationships);
                        if (implicitlyAffectedDependents.Any())
                        {
                            var relationshipHelper = TypeHelper.CreateInstanceOfOpenType(typeof(UpdatedRelationshipHelper<>), entityType, implicitlyAffectedDependents);
                            CallHook(nestedHookcontainer, ResourceHook.BeforeImplicitUpdateRelationship, new object[] { relationshipHelper, pipeline });
                        }
                    }
                }
            }
        }

        private RelationshipProxy GetInverseRelationship(RelationshipProxy proxy)
        {
            var attr = _graph.GetContextEntity(proxy.DependentType).Relationships.Single(r => r.InternalRelationshipName == proxy.Attribute.InverseNavigation);
            return new RelationshipProxy(attr, proxy.PrincipalType, proxy.DependentType, false);
        }

        private Dictionary<RelationshipProxy, List<IIdentifiable>> LoadImplicitlyAffected(
            Dictionary<RelationshipProxy, List<IIdentifiable>> relationships, List<IIdentifiable> dependentEntities = null)
        {

            var implicitlyAffected = new Dictionary<RelationshipProxy, List<IIdentifiable>>();
            relationships.Where(p => !(p.Key.Attribute is HasManyThroughAttribute)).ToList().ForEach(kvp =>
            {
                var relationship = kvp.Key;
                var principalEntities = kvp.Value;
                var principalEntityType = relationship.PrincipalType;
                var includedPrincipals = _meta.LoadDbValues(principalEntities, new List<RelationshipProxy>() { relationship }, principalEntityType).Cast<IIdentifiable>().ToList();
                foreach (var e in includedPrincipals)
                {
                    IList dbDependentEntityList = null;
                    var relationshipValue = relationship.GetValue(e);
                    if (!(relationshipValue is IList))
                    {
                        dbDependentEntityList = TypeHelper.CreateListFor(relationship.DependentType);
                        if (relationshipValue != null) dbDependentEntityList.Add(relationshipValue);
                    } else
                    {
                        dbDependentEntityList = (IList)relationshipValue;
                    }
                    var dbDependentEntityListCasted = dbDependentEntityList.Cast<IIdentifiable>().ToList();
                    if (dependentEntities != null) dbDependentEntityListCasted = dbDependentEntityListCasted.Except(dependentEntities, Comparer).ToList();

                    if (dbDependentEntityListCasted.Any())
                    {
                        if (!implicitlyAffected.TryGetValue(relationship, out List<IIdentifiable> affected))
                        {
                            affected = new List<IIdentifiable>();
                            implicitlyAffected[relationship] = affected;
                        }
                        affected.AddRange(dbDependentEntityListCasted);
                    }
                }
            });
            return implicitlyAffected.ToDictionary(kvp => kvp.Key, kvp => new HashSet<IIdentifiable>(kvp.Value).ToList());
        }






        private IList LoadDbValues(IList entities, List<RelationshipProxy> relationships, Type entityType, ResourceHook hook)
        {
            if (_meta.ShouldLoadDbValues(entityType, hook))
            {
                var list = _meta.LoadDbValues(entities, relationships, entityType) ?? new List<IIdentifiable>();
                return TypeHelper.ConvertCollection((IEnumerable<object>)list, entityType);
            }
            return null;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> AfterUpdate<TEntity>(IEnumerable<TEntity> entities, ResourceAction pipeline) where TEntity : class, IIdentifiable
        {
            return entities;
        }



        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> AfterDelete<TEntity>(IEnumerable<TEntity> entities, ResourceAction pipeline, bool succeeded) where TEntity : class, IIdentifiable
        {

            return entities;
        }



        /// <summary>
        /// checks that the collection does not contain more than one item when
        /// relevant (eg AfterRead from GetSingle pipeline).
        /// </summary>
        /// <param name="returnedList"> The collection returned from the hook</param>
        /// <param name="pipeline">The pipeine from which the hook was fired</param>
        protected void ValidateHookResponse(object returnedList, ResourceAction pipeline = 0)
        {
            if (pipeline != ResourceAction.None && SingleActions.Contains(pipeline) && ((IEnumerable)returnedList).Cast<object>().Count() > 1)
            {
                throw new ApplicationException("The returned collection from this hook may only contain one item in the case of the" +
                    pipeline.ToString() + "pipeline");
            }
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

