using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IResourceHookMetaInfo
    {
        IEnumerable<RelationshipAttribute> GetMetaEntries(IIdentifiable currentLayerEntity);
        IResourceHookContainer<IIdentifiable> GetResourceHookContainer(Type targetEntity, ResourceHook hook = ResourceHook.None);
        IResourceHookContainer<TEntity> GetResourceHookContainer<TEntity>(ResourceHook hook = ResourceHook.None) where TEntity : class, IIdentifiable;
        Dictionary<string, RelationshipAttribute> UpdateMetaInformation(IEnumerable<Type> nextLayerTypes, ResourceHook hook = ResourceHook.None);
        Dictionary<string, RelationshipAttribute> UpdateMetaInformation(IEnumerable<Type> nextLayerTypes, IEnumerable<ResourceHook> hooks);
    }

    public class ResourceHookMetaInfo : IResourceHookMetaInfo
    {

        protected readonly IGenericProcessorFactory _genericProcessorFactory;
        protected readonly IResourceGraph _graph;
        protected readonly Dictionary<Type, IResourceHookContainer<IIdentifiable>> _hookContainers;
        protected readonly List<ResourceHook> _targetedHooksForRelatedEntities;
        protected Dictionary<string, RelationshipAttribute> _meta;

        public ResourceHookMetaInfo(
            IGenericProcessorFactory genericProcessorFactory,
            IResourceGraph graph
            )
        {
            _genericProcessorFactory = genericProcessorFactory;
            _graph = graph;
            _meta = new Dictionary<string, RelationshipAttribute>();
            _hookContainers = new Dictionary<Type, IResourceHookContainer<IIdentifiable>>();
            _targetedHooksForRelatedEntities = new List<ResourceHook>();
        }


        public IEnumerable<RelationshipAttribute> GetMetaEntries(IIdentifiable currentLayerEntity)
        {
            foreach (string metaKey in _meta.Keys)
            {
                var attribute = _meta[metaKey];

                /// because currentLayer is not type-homogeneous (which is 
                /// why we need to use IIdentifiable for the list type of 
                /// that layer), we need to check if relatedType is really 
                /// related to parentType. We do this through comparison of Metakey
                string requiredMetaKey = CreateMetaKey(attribute, currentLayerEntity.GetType());
                if (metaKey != requiredMetaKey) continue;
                yield return attribute;
            }
        }

        /// <summary>
        /// For a particular ResourceHook and for a given model type, checks if 
        /// the ResourceDefinition has an implementation for the hook
        /// and if so, return it.
        /// 
        /// Also caches the retrieves containers so we don't need to reflectively
        /// instantiate multiple times.
        /// </summary>
        /// <returns>The resource definition.</returns>
        /// <param name="targetEntity">Target entity type</param>
        /// <param name="hook">The hook to get a ResourceDefinition for.</param>
        public IResourceHookContainer<IIdentifiable> GetResourceHookContainer(Type targetEntity, ResourceHook hook = ResourceHook.None)
        {
            /// checking the cache if we have a reference for the requested container, 
            /// regardless of the hook we will use it for. If the value is null, 
            /// it means there was no implementation IResourceHookContainer at all, 
            /// so we need not even bother.
            if (!_hookContainers.TryGetValue(targetEntity, out IResourceHookContainer<IIdentifiable> container))
            {
                container = (_genericProcessorFactory.GetProcessor<IResourceHookContainer>(typeof(IResourceHookContainer<>), targetEntity)) as IResourceHookContainer<IIdentifiable>;
                _hookContainers[targetEntity] = container;
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
                if (container.ShouldExecuteHook(targetHook)) return container;
            }
            return null;

        }

        /// <summary>
        /// For a particular ResourceHook and for a given model type, checks if 
        /// the ResourceDefinition has an implementation for the hook
        /// and if so, return it.
        /// 
        /// Also caches the retrieves containers so we don't need to reflectively
        /// instantiate multiple times.
        /// </summary>
        /// <returns>The resource definition.</returns>
        /// <typeparam name="TEntity">Target entity type</typeparam>
        /// <param name="hook">The hook to get a ResourceDefinition for.</param>
        public IResourceHookContainer<TEntity> GetResourceHookContainer<TEntity>(ResourceHook hook = ResourceHook.None) where TEntity : class, IIdentifiable
        {
            return (IResourceHookContainer<TEntity>)GetResourceHookContainer(typeof(TEntity), hook);
        }



        /// <summary>
        /// Creates a (helper) dictionary containing meta information needed for
        /// the traversal of the next layer. It contains as 
        ///     keys:   Type, namely typeof(TRelatedType) that will occur in the traversal 
        ///             of the next layer,
        ///     values: a Tuple of 
        ///                * RelationshipAttribute (that contains getters and setters)
        ///                * IResourceHookExecutor{TRelatedType} to access the actual (nested) hook
        /// </summary>
        /// <returns>The meta dict.</returns>
        /// <param name="nextLayerTypes">Unique list of types to extract metadata from</param>
        /// <param name="hooks">The target resource hook types</param>
        public Dictionary<string, RelationshipAttribute>
            UpdateMetaInformation(
            IEnumerable<Type> nextLayerTypes,
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

            foreach (ResourceHook targetHook in hooks)
            {
                foreach (Type targetType in nextLayerTypes)
                {
                    var contextEntity = _graph.GetContextEntity(targetType);
                    var relationshipsForContextEntity = contextEntity.Relationships.ToDictionary(
                                            attr => CreateMetaKey(attr, targetType, checkForDuplicates: true),
                                            attr => attr);
                    /// keep only the meta info we really need for the traversal of the next layer
                    /// also remove duplicates.
                    PruneMetaDictionary(relationshipsForContextEntity, targetHook);
                    _meta = _meta.Concat(relationshipsForContextEntity)
                                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
            }


            return _meta;

        }


        void CheckForTargetHookExistence()
        {
            if (!_targetedHooksForRelatedEntities.Any())
                throw new InvalidOperationException("Something is not right in the breadth first traversal of resource hook: " +
                    "trying to get meta information when no allowed hooks are set");
        }

        /// <summary>
        /// Creates a (helper) dictionary containing meta information needed for
        /// the traversal of the next layer. It contains as 
        ///     keys:   Type, namely typeof(TRelatedType) that will occur in the traversal 
        ///             of the next layer,
        ///     values: a Tuple of 
        ///                * RelationshipAttribute (that contains getters and setters)
        ///                * IResourceHookExecutor{TRelatedType} to access the actual (nested) hook
        /// </summary>
        /// <returns>The meta dict.</returns>
        /// <param name="nextLayerTypes">Unique list of types to extract metadata from</param>
        /// <param name="hook">The target resource hook type</param>
        public Dictionary<string, RelationshipAttribute>
            UpdateMetaInformation(
            IEnumerable<Type> nextLayerTypes,
            ResourceHook hook = ResourceHook.None)
        {
            var targetHooks = (hook == ResourceHook.None) ? _targetedHooksForRelatedEntities : new List<ResourceHook> { hook };
            return UpdateMetaInformation(nextLayerTypes, targetHooks);
       }



        /// <summary>
        /// Creates the key for the meta dict. The RelationshipAttribute that is
        /// in the value of the meta dict is specific for a particular related type
        /// AS WELL AS parent type. This is reflected by the format of the meta key.
        /// </summary>
        /// <returns>The meta key.</returns>
        /// <param name="attr">Relationship attribute</param>
        /// <param name="parentType">Parent type.</param>
        string CreateMetaKey(RelationshipAttribute attr, Type parentType, bool checkForDuplicates = false)
        {
            var relationType = attr.IsHasOne ? "has-one" : "has-many";
            string newKey = $"{parentType.Name} {relationType} {attr.RelationshipPath}";
            if (checkForDuplicates && _meta.ContainsKey(newKey))
            {
                return $"DUPLICATE-{Guid.NewGuid()}";
            }
            return newKey;
        }

        /// <summary>
        /// Gets rid of keys in the meta dict that won't be needed for the next layer.
        /// 
        /// It does so by:
        ///     1)  checking if there was at all a IResourceHookExecutor 
        ///         implemented for this type (ResourceDefinition by default);
        ///     2)  then checking if there is a implementation of the particular
        ///         target hook. 
        /// </summary>
        void PruneMetaDictionary(
            Dictionary<string, RelationshipAttribute> meta,
            ResourceHook targetHook)
        {
            var dupes = meta.Where(pair => pair.Key.Contains("DUPLICATE")).Select(pair => pair.Key).ToArray();
            foreach (string target in dupes)
            {
                meta.Remove(target);
            }
            var noHookImplementation = meta.Where(pair => GetResourceHookContainer(pair.Value.Type, targetHook) == null).Select(pair => pair.Key).ToArray();
            foreach (string target in noHookImplementation)
            {
                meta.Remove(target);
            }
        }


    }
}