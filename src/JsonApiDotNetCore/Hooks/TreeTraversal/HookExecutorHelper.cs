using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using PrincipalType = System.Type;
using DependentType = System.Type;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Internal
{
    /// <inheritdoc/>
    public class HookExecutorHelper : IHookExecutorHelper
    {
        protected readonly IGenericProcessorFactory _genericProcessorFactory;
        protected readonly IResourceGraph _graph;
        protected readonly Dictionary<DependentType, IResourceHookContainer> _hookContainers;
        protected readonly Dictionary<DependentType, IHooksDiscovery> _hookDiscoveries;
        protected readonly List<ResourceHook> _targetedHooksForRelatedEntities;
        protected readonly IJsonApiContext _context;
        protected Dictionary<PrincipalType, List<RelationshipProxy>> _meta;

        public HookExecutorHelper(
            IGenericProcessorFactory genericProcessorFactory,
            IResourceGraph graph,
            IJsonApiContext context = null
            )
        {
            _genericProcessorFactory = genericProcessorFactory;
            _graph = graph;
            _context = context;
            _meta = new Dictionary<DependentType, List<RelationshipProxy>>();
            _hookContainers = new Dictionary<DependentType, IResourceHookContainer>();
            _hookDiscoveries = new Dictionary<DependentType, IHooksDiscovery>();
            _targetedHooksForRelatedEntities = new List<ResourceHook>();
        }

        /// <inheritdoc/>
        public IResourceHookContainer GetResourceHookContainer(DependentType dependentType, ResourceHook hook = ResourceHook.None)
        {
            /// checking the cache if we have a reference for the requested container, 
            /// regardless of the hook we will use it for. If the value is null, 
            /// it means there was no implementation IResourceHookContainer at all, 
            /// so we need not even bother.
            if (!_hookContainers.TryGetValue(dependentType, out IResourceHookContainer container))
            {
                container = (_genericProcessorFactory.GetProcessor<IResourceHookContainer>(typeof(ResourceDefinition<>), dependentType));
                _hookContainers[dependentType] = container;
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
                if (ShouldExecuteHook(dependentType, targetHook)) return container;
            }
            return null;

        }

        /// <inheritdoc/>
        public IResourceHookContainer<TEntity> GetResourceHookContainer<TEntity>(ResourceHook hook = ResourceHook.None) where TEntity : class, IIdentifiable
        {
            return (IResourceHookContainer<TEntity>)GetResourceHookContainer(typeof(TEntity), hook);
        }


        /// <inheritdoc/>
        public IList  LoadDbValues(IList entities, List<RelationshipProxy> relationships, Type entityType)
        {
            var paths = relationships.Select(p => p.Attribute.RelationshipPath).ToArray();
            var idType = GetIdentifierType(entityType);
            var parameterizedGetWhere = GetType()
                    .GetMethod(nameof(GetWhereAndInclude), BindingFlags.NonPublic | BindingFlags.Instance)
                    .MakeGenericMethod(entityType, idType);
            var casted = ((IEnumerable<object>)entities).Cast<IIdentifiable>();
            var ids = TypeHelper.ConvertListType(casted.Select(e => e.StringId).ToList(), idType);
            var values = (IList)parameterizedGetWhere.Invoke(this, new object[] { ids, paths });
            return values;
        }

        public bool ShouldLoadDbValues(DependentType entityType, ResourceHook hook)
        {
            var discovery = GetHookDiscovery(entityType);

            if (discovery.DatabaseDiffDisabledHooks.Contains(hook))
            {
                return false;
            }
            else if (discovery.DatabaseDiffEnabledHooks.Contains(hook))
            {
                return true;
            }
            else
            {
                return _context.Options.DatabaseValuesInDiffs;
            }

        }

        public bool ShouldExecuteHook(DependentType entityType, ResourceHook hook)
        {
            var discovery = GetHookDiscovery(entityType);
            return discovery.ImplementedHooks.Contains(hook);
        }

        /// <summary>
        /// Gets the type from relationship attribute. If the attribute is 
        /// HasManyThrough, and the jointable entity is identifiable, then the target
        /// type is the joinentity instead of the righthand side, because hooks might be 
        /// implemented for the jointable entity.
        /// </summary>
        /// <returns>The target type for traversal</returns>
        /// <param name="attr">Relationship attribute</param>
        protected DependentType GetDependentTypeFromRelationship(RelationshipAttribute attr)
        {
            if (attr is HasManyThroughAttribute throughAttr)
            {
                if (typeof(IIdentifiable).IsAssignableFrom(throughAttr.ThroughType))
                {
                    return throughAttr.ThroughType;
                }
                return attr.DependentType;
            }
            return attr.DependentType;
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

        protected IEnumerable<TEntity> GetWhereAndInclude<TEntity, TId>(IEnumerable<TId> ids, string[] relationshipPaths) where TEntity : class, IIdentifiable<TId>
        {
            var repo = GetRepository<TEntity, TId>();
            var query = repo.GetQueryable().Where(e => ids.Contains(e.Id));
            foreach (var path in relationshipPaths){
                query = query.Include(path);
            }
            return query.ToList();
        }


        IEntityReadRepository<TEntity, TId> GetRepository<TEntity, TId>() where TEntity : class, IIdentifiable<TId>
        {
            var openType = typeof(TId) == typeof(Guid) ? typeof(IGuidEntityRepository<>) : typeof(IEntityRepository<>);
            return _genericProcessorFactory.GetProcessor<IEntityReadRepository<TEntity, TId>>(openType, typeof(TEntity));

        }

    }
}