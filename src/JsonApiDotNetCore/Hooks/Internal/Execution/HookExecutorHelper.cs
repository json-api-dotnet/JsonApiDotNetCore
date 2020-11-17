using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Hooks.Internal.Discovery;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using LeftType = System.Type;
using RightType = System.Type;

namespace JsonApiDotNetCore.Hooks.Internal.Execution
{
    /// <inheritdoc />
    internal sealed class HookExecutorHelper : IHookExecutorHelper
    {
        private readonly IdentifiableComparer _comparer = IdentifiableComparer.Instance;
        private readonly IJsonApiOptions _options;
        private readonly IGenericServiceFactory _genericProcessorFactory;
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly Dictionary<RightType, IResourceHookContainer> _hookContainers;
        private readonly Dictionary<RightType, IHooksDiscovery> _hookDiscoveries;
        private readonly List<ResourceHook> _targetedHooksForRelatedResources;

        public HookExecutorHelper(IGenericServiceFactory genericProcessorFactory, IResourceContextProvider resourceContextProvider, IJsonApiOptions options)
        {
            _options = options;
            _genericProcessorFactory = genericProcessorFactory;
            _resourceContextProvider = resourceContextProvider;
            _hookContainers = new Dictionary<RightType, IResourceHookContainer>();
            _hookDiscoveries = new Dictionary<RightType, IHooksDiscovery>();
            _targetedHooksForRelatedResources = new List<ResourceHook>();
        }

        /// <inheritdoc />
        public IResourceHookContainer GetResourceHookContainer(RightType targetResource, ResourceHook hook = ResourceHook.None)
        {
            // checking the cache if we have a reference for the requested container, 
            // regardless of the hook we will use it for. If the value is null, 
            // it means there was no implementation IResourceHookContainer at all, 
            // so we need not even bother.
            if (!_hookContainers.TryGetValue(targetResource, out IResourceHookContainer container))
            {
                container = _genericProcessorFactory.Get<IResourceHookContainer>(typeof(ResourceHooksDefinition<>), targetResource);
                _hookContainers[targetResource] = container;
            }
            if (container == null) return null;

            // if there was a container, first check if it implements the hook we 
            // want to use it for.
            List<ResourceHook> targetHooks;
            if (hook == ResourceHook.None)
            {
                CheckForTargetHookExistence();
                targetHooks = _targetedHooksForRelatedResources;
            }
            else
            {
                targetHooks = new List<ResourceHook> { hook };
            }

            foreach (ResourceHook targetHook in targetHooks)
            {
                if (ShouldExecuteHook(targetResource, targetHook)) return container;
            }
            return null;
        }

        /// <inheritdoc />
        public IResourceHookContainer<TResource> GetResourceHookContainer<TResource>(ResourceHook hook = ResourceHook.None) where TResource : class, IIdentifiable
        {
            return (IResourceHookContainer<TResource>)GetResourceHookContainer(typeof(TResource), hook);
        }

        public IEnumerable LoadDbValues(LeftType resourceTypeForRepository, IEnumerable resources, ResourceHook hook, params RelationshipAttribute[] relationshipsToNextLayer)
        {
            var idType = TypeHelper.GetIdType(resourceTypeForRepository);
            var parameterizedGetWhere = GetType()
                    .GetMethod(nameof(GetWhereAndInclude), BindingFlags.NonPublic | BindingFlags.Instance)
                    .MakeGenericMethod(resourceTypeForRepository, idType);
            var cast = ((IEnumerable<object>)resources).Cast<IIdentifiable>();
            var ids = TypeHelper.CopyToList(cast.Select(i => i.GetTypedId()), idType);
            var values = (IEnumerable)parameterizedGetWhere.Invoke(this, new object[] { ids, relationshipsToNextLayer });
            if (values == null) return null;
            return (IEnumerable)Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(resourceTypeForRepository), TypeHelper.CopyToList(values, resourceTypeForRepository));
        }

        public HashSet<TResource> LoadDbValues<TResource>(IEnumerable<TResource> resources, ResourceHook hook, params RelationshipAttribute[] relationships) where TResource : class, IIdentifiable
        {
            var resourceType = typeof(TResource);
            var dbValues = LoadDbValues(resourceType, resources, hook, relationships)?.Cast<TResource>();
            if (dbValues == null) return null;
            return new HashSet<TResource>(dbValues);
        }

        public bool ShouldLoadDbValues(Type resourceType, ResourceHook hook)
        {
            var discovery = GetHookDiscovery(resourceType);
            if (discovery.DatabaseValuesDisabledHooks.Contains(hook))
                return false;
            if (discovery.DatabaseValuesEnabledHooks.Contains(hook))
                return true;
            return _options.LoadDatabaseValues;
        }

        private bool ShouldExecuteHook(RightType resourceType, ResourceHook hook)
        {
            var discovery = GetHookDiscovery(resourceType);
            return discovery.ImplementedHooks.Contains(hook);
        }

        private void CheckForTargetHookExistence()
        {
            if (!_targetedHooksForRelatedResources.Any())
                throw new InvalidOperationException("Something is not right in the breadth first traversal of resource hook: " +
                    "trying to get meta information when no allowed hooks are set");
        }

        private IHooksDiscovery GetHookDiscovery(Type resourceType)
        {
            if (!_hookDiscoveries.TryGetValue(resourceType, out IHooksDiscovery discovery))
            {
                discovery = _genericProcessorFactory.Get<IHooksDiscovery>(typeof(IHooksDiscovery<>), resourceType);
                _hookDiscoveries[resourceType] = discovery;
            }
            return discovery;
        }

        private IEnumerable<TResource> GetWhereAndInclude<TResource, TId>(IReadOnlyCollection<TId> ids, RelationshipAttribute[] relationshipsToNextLayer) where TResource : class, IIdentifiable<TId>
        {
            if (!ids.Any())
            {
                return Array.Empty<TResource>();
            }

            var resourceContext = _resourceContextProvider.GetResourceContext<TResource>();
            var filterExpression = CreateFilterByIds(ids, resourceContext);

            var queryLayer = new QueryLayer(resourceContext)
            {
                Filter = filterExpression
            };

            var chains = relationshipsToNextLayer.Select(relationship => new ResourceFieldChainExpression(relationship)).ToList();
            if (chains.Any())
            {
                queryLayer.Include = IncludeChainConverter.FromRelationshipChains(chains);
            }

            var repository = GetRepository<TResource, TId>();
            return repository.GetAsync(queryLayer, CancellationToken.None).Result;
        }

        private static FilterExpression CreateFilterByIds<TId>(IReadOnlyCollection<TId> ids, ResourceContext resourceContext)
        {
            var idAttribute = resourceContext.Attributes.Single(attr => attr.Property.Name == nameof(Identifiable.Id));
            var idChain = new ResourceFieldChainExpression(idAttribute);

            if (ids.Count == 1)
            {
                var constant = new LiteralConstantExpression(ids.Single().ToString());
                return new ComparisonExpression(ComparisonOperator.Equals, idChain, constant);
            }

            var constants = ids.Select(id => new LiteralConstantExpression(id.ToString())).ToList();
            return new EqualsAnyOfExpression(idChain, constants);
        }

        private IResourceReadRepository<TResource, TId> GetRepository<TResource, TId>() where TResource : class, IIdentifiable<TId>
        {
            return _genericProcessorFactory.Get<IResourceReadRepository<TResource, TId>>(typeof(IResourceReadRepository<,>), typeof(TResource), typeof(TId));
        }

        public Dictionary<RelationshipAttribute, IEnumerable> LoadImplicitlyAffected(
            Dictionary<RelationshipAttribute, IEnumerable> leftResourcesByRelation,
            IEnumerable existingRightResources = null)
        {
            var implicitlyAffected = new Dictionary<RelationshipAttribute, IEnumerable>();
            foreach (var kvp in leftResourcesByRelation)
            {
                if (IsHasManyThrough(kvp, out var lefts, out var relationship)) continue;

                // note that we don't have to check if BeforeImplicitUpdate hook is implemented. If not, it wont ever get here.
                var includedLefts = LoadDbValues(relationship.LeftType, lefts, ResourceHook.BeforeImplicitUpdateRelationship, relationship);

                foreach (IIdentifiable ip in includedLefts)
                {
                    IList dbRightResourceList = TypeHelper.CreateListFor(relationship.RightType);
                    var relationshipValue = relationship.GetValue(ip);
                    if (!(relationshipValue is IEnumerable))
                    {
                        if (relationshipValue != null) dbRightResourceList.Add(relationshipValue);
                    }
                    else
                    {
                        foreach (var item in (IEnumerable) relationshipValue)
                        {
                            dbRightResourceList.Add(item);
                        }
                    }

                    var dbRightResourceListCast = dbRightResourceList.Cast<IIdentifiable>().ToList();
                    if (existingRightResources != null) dbRightResourceListCast = dbRightResourceListCast.Except(existingRightResources.Cast<IIdentifiable>(), _comparer).ToList();

                    if (dbRightResourceListCast.Any())
                    {
                        if (!implicitlyAffected.TryGetValue(relationship, out IEnumerable affected))
                        {
                            affected = TypeHelper.CreateListFor(relationship.RightType);
                            implicitlyAffected[relationship] = affected;
                        }

                        foreach (var item in dbRightResourceListCast)
                        {
                            ((IList)affected).Add(item);
                        }
                    }
                }
            }

            return implicitlyAffected.ToDictionary(kvp => kvp.Key, kvp => TypeHelper.CreateHashSetFor(kvp.Key.RightType, kvp.Value));
        }

        private bool IsHasManyThrough(KeyValuePair<RelationshipAttribute, IEnumerable> kvp,
            out IEnumerable resources,
            out RelationshipAttribute attr)
        {
            attr = kvp.Key;
            resources = kvp.Value;
            return kvp.Key is HasManyThroughAttribute;
        }
    }
}

