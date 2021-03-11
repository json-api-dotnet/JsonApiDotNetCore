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
    internal sealed class HookContainerProvider : IHookContainerProvider
    {
        private static readonly HooksCollectionConverter CollectionConverter = new HooksCollectionConverter();
        private static readonly HooksObjectFactory ObjectFactory = new HooksObjectFactory();
        private static readonly IncludeChainConverter IncludeChainConverter = new IncludeChainConverter();

        private readonly IdentifiableComparer _comparer = IdentifiableComparer.Instance;
        private readonly IJsonApiOptions _options;
        private readonly IGenericServiceFactory _genericServiceFactory;
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly Dictionary<RightType, IResourceHookContainer> _hookContainers;
        private readonly Dictionary<RightType, IHooksDiscovery> _hookDiscoveries;
        private readonly List<ResourceHook> _targetedHooksForRelatedResources;

        public HookContainerProvider(IGenericServiceFactory genericServiceFactory, IResourceContextProvider resourceContextProvider, IJsonApiOptions options)
        {
            _options = options;
            _genericServiceFactory = genericServiceFactory;
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
                container = _genericServiceFactory.Get<IResourceHookContainer>(typeof(ResourceHooksDefinition<>), targetResource);
                _hookContainers[targetResource] = container;
            }

            if (container == null)
            {
                return null;
            }

            // if there was a container, first check if it implements the hook we
            // want to use it for.
            IEnumerable<ResourceHook> targetHooks;

            if (hook == ResourceHook.None)
            {
                CheckForTargetHookExistence();
                targetHooks = _targetedHooksForRelatedResources;
            }
            else
            {
                targetHooks = hook.AsEnumerable();
            }

            foreach (ResourceHook targetHook in targetHooks)
            {
                if (ShouldExecuteHook(targetResource, targetHook))
                {
                    return container;
                }
            }

            return null;
        }

        /// <inheritdoc />
        public IResourceHookContainer<TResource> GetResourceHookContainer<TResource>(ResourceHook hook = ResourceHook.None)
            where TResource : class, IIdentifiable
        {
            return (IResourceHookContainer<TResource>)GetResourceHookContainer(typeof(TResource), hook);
        }

        public IEnumerable LoadDbValues(LeftType resourceTypeForRepository, IEnumerable resources, params RelationshipAttribute[] relationshipsToNextLayer)
        {
            LeftType idType = ObjectFactory.GetIdType(resourceTypeForRepository);

            MethodInfo parameterizedGetWhere =
                GetType().GetMethod(nameof(GetWhereWithInclude), BindingFlags.NonPublic | BindingFlags.Instance)!.MakeGenericMethod(resourceTypeForRepository,
                    idType);

            IEnumerable<object> resourceIds = ((IEnumerable<object>)resources).Cast<IIdentifiable>().Select(resource => resource.GetTypedId());
            IList idsAsList = CollectionConverter.CopyToList(resourceIds, idType);
            var values = (IEnumerable)parameterizedGetWhere.Invoke(this, ArrayFactory.Create<object>(idsAsList, relationshipsToNextLayer));

            return values == null ? null : CollectionConverter.CopyToHashSet(values, resourceTypeForRepository);
        }

        public bool ShouldLoadDbValues(LeftType resourceType, ResourceHook hook)
        {
            IHooksDiscovery discovery = GetHookDiscovery(resourceType);

            if (discovery.DatabaseValuesDisabledHooks.Contains(hook))
            {
                return false;
            }

            if (discovery.DatabaseValuesEnabledHooks.Contains(hook))
            {
                return true;
            }

            return _options.LoadDatabaseValues;
        }

        private bool ShouldExecuteHook(RightType resourceType, ResourceHook hook)
        {
            IHooksDiscovery discovery = GetHookDiscovery(resourceType);
            return discovery.ImplementedHooks.Contains(hook);
        }

        private void CheckForTargetHookExistence()
        {
            if (!_targetedHooksForRelatedResources.Any())
            {
                throw new InvalidOperationException("Something is not right in the breadth first traversal of resource hook: " +
                    "trying to get meta information when no allowed hooks are set");
            }
        }

        private IHooksDiscovery GetHookDiscovery(LeftType resourceType)
        {
            if (!_hookDiscoveries.TryGetValue(resourceType, out IHooksDiscovery discovery))
            {
                discovery = _genericServiceFactory.Get<IHooksDiscovery>(typeof(IHooksDiscovery<>), resourceType);
                _hookDiscoveries[resourceType] = discovery;
            }

            return discovery;
        }

        private IEnumerable<TResource> GetWhereWithInclude<TResource, TId>(IReadOnlyCollection<TId> ids, RelationshipAttribute[] relationshipsToNextLayer)
            where TResource : class, IIdentifiable<TId>
        {
            if (!ids.Any())
            {
                return Array.Empty<TResource>();
            }

            ResourceContext resourceContext = _resourceContextProvider.GetResourceContext<TResource>();
            FilterExpression filterExpression = CreateFilterByIds(ids, resourceContext);

            var queryLayer = new QueryLayer(resourceContext)
            {
                Filter = filterExpression
            };

            List<ResourceFieldChainExpression> chains = relationshipsToNextLayer.Select(relationship => new ResourceFieldChainExpression(relationship))
                .ToList();

            if (chains.Any())
            {
                queryLayer.Include = IncludeChainConverter.FromRelationshipChains(chains);
            }

            IResourceReadRepository<TResource, TId> repository = GetRepository<TResource, TId>();
            return repository.GetAsync(queryLayer, CancellationToken.None).Result;
        }

        private static FilterExpression CreateFilterByIds<TId>(IReadOnlyCollection<TId> ids, ResourceContext resourceContext)
        {
            AttrAttribute idAttribute = resourceContext.Attributes.Single(attr => attr.Property.Name == nameof(Identifiable.Id));
            var idChain = new ResourceFieldChainExpression(idAttribute);

            if (ids.Count == 1)
            {
                var constant = new LiteralConstantExpression(ids.Single().ToString());
                return new ComparisonExpression(ComparisonOperator.Equals, idChain, constant);
            }

            List<LiteralConstantExpression> constants = ids.Select(id => new LiteralConstantExpression(id.ToString())).ToList();
            return new EqualsAnyOfExpression(idChain, constants);
        }

        private IResourceReadRepository<TResource, TId> GetRepository<TResource, TId>()
            where TResource : class, IIdentifiable<TId>
        {
            return _genericServiceFactory.Get<IResourceReadRepository<TResource, TId>>(typeof(IResourceReadRepository<,>), typeof(TResource), typeof(TId));
        }

        public IDictionary<RelationshipAttribute, IEnumerable> LoadImplicitlyAffected(IDictionary<RelationshipAttribute, IEnumerable> leftResourcesByRelation,
            IEnumerable existingRightResources = null)
        {
            List<IIdentifiable> existingRightResourceList = existingRightResources?.Cast<IIdentifiable>().ToList();

            var implicitlyAffected = new Dictionary<RelationshipAttribute, IEnumerable>();

            foreach (KeyValuePair<RelationshipAttribute, IEnumerable> pair in leftResourcesByRelation)
            {
                RelationshipAttribute relationship = pair.Key;
                IEnumerable lefts = pair.Value;

                if (relationship is HasManyThroughAttribute)
                {
                    continue;
                }

                // note that we don't have to check if BeforeImplicitUpdate hook is implemented. If not, it wont ever get here.
                IEnumerable includedLefts = LoadDbValues(relationship.LeftType, lefts, relationship);

                AddToImplicitlyAffected(includedLefts, relationship, existingRightResourceList, implicitlyAffected);
            }

            return implicitlyAffected.ToDictionary(pair => pair.Key, pair => CollectionConverter.CopyToHashSet(pair.Value, pair.Key.RightType));
        }

        private void AddToImplicitlyAffected(IEnumerable includedLefts, RelationshipAttribute relationship, List<IIdentifiable> existingRightResourceList,
            Dictionary<RelationshipAttribute, IEnumerable> implicitlyAffected)
        {
            foreach (IIdentifiable ip in includedLefts)
            {
                object relationshipValue = relationship.GetValue(ip);
                ICollection<IIdentifiable> dbRightResources = CollectionConverter.ExtractResources(relationshipValue);

                if (existingRightResourceList != null)
                {
                    dbRightResources = dbRightResources.Except(existingRightResourceList, _comparer).ToList();
                }

                if (dbRightResources.Any())
                {
                    if (!implicitlyAffected.TryGetValue(relationship, out IEnumerable affected))
                    {
                        affected = CollectionConverter.CopyToList(Array.Empty<object>(), relationship.RightType);
                        implicitlyAffected[relationship] = affected;
                    }

                    AddToList((IList)affected, dbRightResources);
                }
            }
        }

        private static void AddToList(IList list, IEnumerable itemsToAdd)
        {
            foreach (object item in itemsToAdd)
            {
                list.Add(item);
            }
        }
    }
}
