using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Extensions;
using PrincipalType = System.Type;
using DependentType = System.Type;
using Microsoft.EntityFrameworkCore;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCore.Internal;

namespace JsonApiDotNetCore.Hooks
{
    /// <inheritdoc/>
    internal class HookExecutorHelper : IHookExecutorHelper
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
            IJsonApiContext context
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

        public IEnumerable LoadDbValues(PrincipalType entityTypeForRepository, IEnumerable entities, ResourceHook hook, params RelationshipProxy[] relationships)
        {
            var paths = relationships.Select(p => p.Attribute.RelationshipPath).ToArray();
            var idType = GetIdentifierType(entityTypeForRepository);
            var parameterizedGetWhere = GetType()
                    .GetMethod(nameof(GetWhereAndInclude), BindingFlags.NonPublic | BindingFlags.Instance)
                    .MakeGenericMethod(entityTypeForRepository, idType);
            var casted = ((IEnumerable<object>)entities).Cast<IIdentifiable>();
            var ids = casted.Select(e => e.StringId).Cast(idType);
            var values = (IEnumerable)parameterizedGetWhere.Invoke(this, new object[] { ids, paths });
            if (values == null) return null;
            return (IEnumerable)Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(entityTypeForRepository), values.Cast(entityTypeForRepository));
        }

        public HashSet<TEntity> LoadDbValues<TEntity>(IEnumerable<TEntity> entities, ResourceHook hook, params RelationshipProxy[] relationships) where TEntity : class, IIdentifiable
        {
            var entityType = typeof(TEntity);
            var dbValues = LoadDbValues(entityType, entities, hook, relationships)?.Cast<TEntity>();
            if (dbValues == null) return null;
            return new HashSet<TEntity>(dbValues);
        }


        public bool ShouldLoadDbValues(Type entityType, ResourceHook hook)
        {
            var discovery = GetHookDiscovery(entityType);

            if (discovery.DatabaseValuesDisabledHooks.Contains(hook))
            {
                return false;
            }
            else if (discovery.DatabaseValuesEnabledHooks.Contains(hook))
            {
                return true;
            }
            else
            {
                return _context.Options.LoadDatabaseValues;
            }
        }

        bool ShouldExecuteHook(DependentType entityType, ResourceHook hook)
        {
            var discovery = GetHookDiscovery(entityType);
            return discovery.ImplementedHooks.Contains(hook);
        }


        void CheckForTargetHookExistence()
        {
            if (!_targetedHooksForRelatedEntities.Any())
                throw new InvalidOperationException("Something is not right in the breadth first traversal of resource hook: " +
                    "trying to get meta information when no allowed hooks are set");
        }

        IHooksDiscovery GetHookDiscovery(Type entityType)
        {
            if (!_hookDiscoveries.TryGetValue(entityType, out IHooksDiscovery discovery))
            {
                discovery = _genericProcessorFactory.GetProcessor<IHooksDiscovery>(typeof(IHooksDiscovery<>), entityType);
                _hookDiscoveries[entityType] = discovery;
            }
            return discovery;
        }

        Type GetIdentifierType(Type entityType)
        {
            return entityType.GetProperty("Id").PropertyType;
        }

        IEnumerable<TEntity> GetWhereAndInclude<TEntity, TId>(IEnumerable<TId> ids, string[] relationshipPaths) where TEntity : class, IIdentifiable<TId>
        {
            var repo = GetRepository<TEntity, TId>();
            var query = repo.Get().Where(e => ids.Contains(e.Id));
            foreach (var path in relationshipPaths)
            {
                query = query.Include(path);
            }
            return query.ToList();
        }

        IEntityReadRepository<TEntity, TId> GetRepository<TEntity, TId>() where TEntity : class, IIdentifiable<TId>
        {
            return _genericProcessorFactory.GetProcessor<IEntityReadRepository<TEntity, TId>>(typeof(IEntityReadRepository<,>), typeof(TEntity), typeof(TId));
        }


        public Dictionary<RelationshipProxy, IEnumerable> LoadImplicitlyAffected(
            Dictionary<RelationshipProxy, IEnumerable> principalEntitiesByRelation,
            IEnumerable existingDependentEntities = null)
        {
            var implicitlyAffected = new Dictionary<RelationshipProxy, IEnumerable>();
            foreach (var kvp in principalEntitiesByRelation)
            {
                if (IsHasManyThrough(kvp, out var principals, out var relationship)) continue;

                // note that we dont't have to check if BeforeImplicitUpdate hook is implemented. If not, it wont ever get here.
                var includedPrincipals = LoadDbValues(relationship.PrincipalType, principals, ResourceHook.BeforeImplicitUpdateRelationship, relationship);

                foreach (IIdentifiable ip in includedPrincipals)
                {
                    IList dbDependentEntityList = null;
                    var relationshipValue = relationship.GetValue(ip);
                    if (!(relationshipValue is IEnumerable))
                    {
                        dbDependentEntityList = TypeHelper.CreateListFor(relationship.DependentType);
                        if (relationshipValue != null) dbDependentEntityList.Add(relationshipValue);
                    }
                    else
                    {
                        dbDependentEntityList = (IList)relationshipValue;
                    }
                    var dbDependentEntityListCasted = dbDependentEntityList.Cast<IIdentifiable>().ToList();
                    if (existingDependentEntities != null) dbDependentEntityListCasted = dbDependentEntityListCasted.Except(existingDependentEntities.Cast<IIdentifiable>(), ResourceHookExecutor.Comparer).ToList();

                    if (dbDependentEntityListCasted.Any())
                    {
                        if (!implicitlyAffected.TryGetValue(relationship, out IEnumerable affected))
                        {
                            affected = TypeHelper.CreateListFor(relationship.DependentType);
                            implicitlyAffected[relationship] = affected;
                        }
                        ((IList)affected).AddRange(dbDependentEntityListCasted);
                    }
                }
            }

            return implicitlyAffected.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        }

        bool IsHasManyThrough(KeyValuePair<RelationshipProxy, IEnumerable> kvp,
            out IEnumerable entities,
            out RelationshipProxy proxy)
        {
            proxy = kvp.Key;
            entities = (kvp.Value);
            return (kvp.Key.Attribute is HasManyThroughAttribute);
        }
    }
}