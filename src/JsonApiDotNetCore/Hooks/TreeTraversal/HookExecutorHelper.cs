using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;


namespace JsonApiDotNetCore.Internal
{
    /// <inheritdoc/>
    public class HookExecutorHelper : IHookExecutorHelper
    {
        protected readonly IGenericProcessorFactory _genericProcessorFactory;
        protected readonly IResourceGraph _graph;
        protected readonly Dictionary<Type, IResourceHookContainer> _hookContainers;
        protected readonly Dictionary<Type, IHooksDiscovery> _hookDiscoveries;
        protected readonly List<ResourceHook> _targetedHooksForRelatedEntities;
        protected readonly IJsonApiContext _context;
        protected Dictionary<Type, List<RelationshipProxy>> _meta;

        public HookExecutorHelper(
            IGenericProcessorFactory genericProcessorFactory,
            IResourceGraph graph,
            IJsonApiContext context = null
            )
        {
            _genericProcessorFactory = genericProcessorFactory;
            _graph = graph;
            _context = context;
            _meta = new Dictionary<Type, List<RelationshipProxy>>();
            _hookContainers = new Dictionary<Type, IResourceHookContainer>();
            _hookDiscoveries = new Dictionary<Type, IHooksDiscovery>();
            _targetedHooksForRelatedEntities = new List<ResourceHook>();
        }


        /// <inheritdoc/>
        public IEnumerable<RelationshipProxy> GetMetaEntries(IIdentifiable currentEntityTreeLayerEntity)
        {
            foreach (Type metaKey in _meta.Keys)
            {
                List<RelationshipProxy> proxies = _meta[metaKey];

                foreach (var proxy in proxies)
                {
                    /// because currentEntityTreeLayer is not type-homogeneous (which is 
                    /// why we need to use IIdentifiable for the list type of 
                    /// that layer), we need to check if relatedType is really 
                    /// related to parentType. We do this through comparison of Metakey
                    string identifier = CreateRelationshipIdentifier(proxy.Attribute, currentEntityTreeLayerEntity.GetType());
                    if (proxy.RelationshipIdentifier != identifier) continue;
                    yield return proxy;
                }

            }
        }

        /// <inheritdoc/>
        public IResourceHookContainer GetResourceHookContainer(Type targetEntityType, ResourceHook hook = ResourceHook.None)
        {
            /// checking the cache if we have a reference for the requested container, 
            /// regardless of the hook we will use it for. If the value is null, 
            /// it means there was no implementation IResourceHookContainer at all, 
            /// so we need not even bother.
            if (!_hookContainers.TryGetValue(targetEntityType, out IResourceHookContainer container))
            {
                container = (_genericProcessorFactory.GetProcessor<IResourceHookContainer>(typeof(ResourceDefinition<>), targetEntityType));
                _hookContainers[targetEntityType] = container;
            }
            if (container == null) return container;

            /// if there was a container, first check if it implements the hook we 
            /// want to use it for.
            List<ResourceHook> targetHooks;
            if (hook == ResourceHook.None)
            {
                CheckForTargetHookExistence();
                targetHooks = _targetedHooksForRelatedEntities;
            }
            else
            {
                targetHooks = new List<ResourceHook>() { hook };
            }

            foreach (ResourceHook targetHook in targetHooks)
            {
                if (ShouldExecuteHook(targetEntityType, targetHook)) return container;
            }
            return null;

        }

        /// <inheritdoc/>
        public IResourceHookContainer<TEntity> GetResourceHookContainer<TEntity>(ResourceHook hook = ResourceHook.None) where TEntity : class, IIdentifiable
        {
            return (IResourceHookContainer<TEntity>)GetResourceHookContainer(typeof(TEntity), hook);
        }

        /// <inheritdoc/>
        public Dictionary<Type, List<RelationshipProxy>>
            UpdateMetaInformation(
            IEnumerable<Type> nextEntityTreeLayerTypes,
            IEnumerable<ResourceHook> hooks)
        {

            if (hooks == null || !hooks.Any())
            {
                CheckForTargetHookExistence();
                hooks = _targetedHooksForRelatedEntities;
            }
            else
            {
                if (!_targetedHooksForRelatedEntities.Any())
                    _targetedHooksForRelatedEntities.AddRange(hooks);
            }


            foreach (Type parentType in nextEntityTreeLayerTypes)
            {
                var contextEntity = _graph.GetContextEntity(parentType);
                foreach (RelationshipAttribute attr in contextEntity.Relationships)
                {
                    var relatedType = GetTargetTypeFromRelationship(attr);

                    bool hasImplementedHooks = false;
                    foreach (ResourceHook targetHook in hooks)
                    {
                        if (GetResourceHookContainer(relatedType, targetHook) != null)
                        {
                            hasImplementedHooks = true;
                            break;
                        }
                    }
                    /// check for the related type if there are any hooks 
                    /// implemented; if not we can skip to the next relationship
                    if (!hasImplementedHooks) continue;

                    /// If we already have detected relationships for the related 
                    /// type in previous iterations, use that list
                    bool newKey = false;
                    if (!_meta.TryGetValue(relatedType, out List<RelationshipProxy> proxies))
                    {
                        proxies = new List<RelationshipProxy>();
                        newKey = true;
                    }

                    var identifier = CreateRelationshipIdentifier(attr, parentType);

                    var isContextRelation = _context?.RelationshipsToUpdate?.Keys.Contains(attr);
                    var proxy = new RelationshipProxy(attr, relatedType, 
                            parentType, identifier, isContextRelation != null && (bool)isContextRelation);

                    /// we might already have covered for this relationship, like 
                    /// in a hierarchical self-refering nested structure 
                    /// (eg folders).
                    if (!proxies.Select( p => p.RelationshipIdentifier).Contains(proxy.RelationshipIdentifier))
                    {
                        proxies.Add(proxy);
                    }
                    if (newKey && proxies.Any())
                    {
                        _meta[relatedType] = proxies;
                    }
                }
            }
            return _meta;
        }


        /// <inheritdoc/>
        public Dictionary<Type, List<RelationshipProxy>>
            UpdateMetaInformation(
            IEnumerable<Type> nextEntityTreeLayerTypes,
            ResourceHook hook = ResourceHook.None)
        {
            var targetHooks = (hook == ResourceHook.None) ? _targetedHooksForRelatedEntities : new List<ResourceHook> { hook };
            return UpdateMetaInformation(nextEntityTreeLayerTypes, targetHooks);
        }

        /// <inheritdoc/>
        public IList GetDatabaseValues(IResourceHookContainer container, IList entities, ResourceHook hook)
        {
            var entityType = TypeHelper.GetListInnerType(entities);
            if (ShouldIncludeDatabaseDiff(entityType, hook))
            {

                var idType = GetIdentifierType(entityType);

                var parameterizedGetter = GetType()
                        .GetMethod("GetValuesFromRepository", BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(entityType, idType);
                var casted = ((IEnumerable<object>)entities).Cast<IIdentifiable>();
                var ids = casted.Select(e => e.StringId);
                return (IList)parameterizedGetter.Invoke(this, new object[] { ids });
            }
            return null;
        }

        /// <inheritdoc/>
        public IEnumerable<TEntity> GetDatabaseValues<TEntity>(
            IResourceHookContainer container,
            IEnumerable<TEntity> entities,
            ResourceHook hook) where TEntity : class, IIdentifiable
        {
            return (IEnumerable<TEntity>)GetDatabaseValues(container, (IList)entities, hook);
        }

        public bool ShouldIncludeDatabaseDiff(Type entityType, ResourceHook hook)
        {
            var discovery = GetHookDiscovery(entityType);

            if (discovery.DatabaseDiffDisabledHooks.Contains(hook))
            {
                return false;
            }
            else if (discovery.DatabaseDiffEnabledHooks.Contains(hook))
            {
                return false;
            }
            else
            {
                return _context.Options.DatabaseValuesInDiffs;
            }

        }

        public bool ShouldExecuteHook(Type entityType, ResourceHook hook)
        {
            var discovery = GetHookDiscovery(entityType);
            return discovery.ImplementedHooks.Contains(hook);
        }

        /// <summary>
        /// relationship attributes for the same relation are not the same object in memory, for some reason.
        /// This identifier provides a way to compare.
        /// </summary>
        /// <returns>The meta key.</returns>
        /// <param name="attr">Relationship attribute</param>
        /// <param name="parentType">Parent type.</param>
        protected string CreateRelationshipIdentifier(RelationshipAttribute attr, Type parentType)
        {
            string relationType;
            string rightHandIdentifier;
            if (attr is HasManyThroughAttribute manyThroughAttr)
            {
                relationType = "has-many-through";
                rightHandIdentifier = manyThroughAttr.ThroughProperty.Name;
            }
            else if (attr is HasManyAttribute manyAttr)
            {
                relationType = "has-many";
                rightHandIdentifier = manyAttr.RelationshipPath;
            }
            else
            {
                relationType = "has-one";
                rightHandIdentifier = attr.RelationshipPath;
            }
            return $"{parentType.Name} {relationType} {rightHandIdentifier}";
        }

        /// <summary>
        /// Gets the type from relationship attribute. If the attribute is 
        /// HasManyThrough, and the jointable entity is identifiable, then the target
        /// type is the joinentity instead of the righthand side, because hooks might be 
        /// implemented for the jointable entity.
        /// </summary>
        /// <returns>The target type for traversal</returns>
        /// <param name="attr">Relationship attribute</param>
        protected Type GetTargetTypeFromRelationship(RelationshipAttribute attr)
        {
            if (attr is HasManyThroughAttribute throughAttr)
            {
                if (typeof(IIdentifiable).IsAssignableFrom(throughAttr.ThroughType))
                {
                    return throughAttr.ThroughType;
                }
                return attr.Type;
            }
            return attr.Type;
        }

        protected void CheckForTargetHookExistence()
        {
            if (!_targetedHooksForRelatedEntities.Any())
                throw new InvalidOperationException("Something is not right in the breadth first traversal of resource hook: " +
                    "trying to get meta information when no allowed hooks are set");
        }

        protected IHooksDiscovery GetHookDiscovery(Type entityType)
        {
            if (!_hookDiscoveries.TryGetValue(entityType, out IHooksDiscovery discovery))
            {
                discovery = _genericProcessorFactory.GetProcessor<IHooksDiscovery>(typeof(IHooksDiscovery<>), entityType);
                _hookDiscoveries[entityType] = discovery;
            }
            return discovery;
        }

        protected Type GetIdentifierType(Type entityType)
        {
            return entityType.GetProperty("Id").PropertyType;
        }

#pragma warning disable IDE0051 // Remove unused private members
        protected IEnumerable<TEntity> GetValuesFromRepository<TEntity, TId>(IEnumerable<TId> ids) where TEntity : class, IIdentifiable<TId>
#pragma warning restore IDE0051 // Remove unused private members
        {
            var openType = typeof(TId) == typeof(Guid) ? typeof(IGuidEntityRepository<>) : typeof(IEntityRepository<>);
            var repo  = _genericProcessorFactory.GetProcessor<IEntityReadRepository<TEntity, TId>>(openType, typeof(TEntity));
            var dbEntities = repo.GetQueryable().Where(e => ids.Contains(e.Id)).ToList();
            return dbEntities;
        }

    }
}