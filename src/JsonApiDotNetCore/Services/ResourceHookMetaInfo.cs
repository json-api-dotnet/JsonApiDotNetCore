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
        IResourceHookContainer<IIdentifiable> GetResourceDefinition(Type targetEntity, ResourceHook hook = ResourceHook.None);
        IResourceHookContainer<TEntity> GetResourceDefinition<TEntity>(ResourceHook hook = ResourceHook.None) where TEntity : class, IIdentifiable;
        Dictionary<string, RelationshipAttribute> UpdateMetaInformation(IEnumerable<Type> nextLayerTypes, ResourceHook hook = ResourceHook.None);
    }

    public class ResourceHookMetaInfo : IResourceHookMetaInfo
    {

        protected readonly IGenericProcessorFactory _genericProcessorFactory;
        protected readonly IResourceGraph _graph;
        protected readonly Dictionary<Type, IResourceHookContainer<IIdentifiable>> _executors;
        protected ResourceHook _hookInTreeTraversal;
        protected Dictionary<string, RelationshipAttribute> _meta;

        public ResourceHookMetaInfo(
            IGenericProcessorFactory genericProcessorFactory,
            IResourceGraph graph
            )
        {
            _genericProcessorFactory = genericProcessorFactory;
            _graph = graph;
            _meta = new Dictionary<string, RelationshipAttribute>();
            _executors = new Dictionary<Type, IResourceHookContainer<IIdentifiable>>();
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
        /// For a particular ResourceHook, checks if the ResourceDefinition has it implemented
        /// and if so, return it.
        /// </summary>
        /// <returns>The resource definition.</returns>
        /// <typeparam name="TEntity">Target entity type</typeparam>
        public IResourceHookContainer<IIdentifiable> GetResourceDefinition(Type targetEntity, ResourceHook hook = ResourceHook.None)
        {
            hook = (hook == ResourceHook.None) ? _hookInTreeTraversal : hook;
            if (!_executors.TryGetValue(targetEntity, out IResourceHookContainer<IIdentifiable> executor))
            {
                executor = (IResourceHookContainer<IIdentifiable>)_genericProcessorFactory.GetProcessor<IResourceDefinition>(typeof(ResourceDefinition<>), targetEntity);
            }
            _executors[targetEntity] = executor;
            if (!executor.ShouldExecuteHook(hook)) executor = null;
            return executor;
        }

        /// <summary>
        /// For a particular ResourceHook, checks if the ResourceDefinition has it implemented
        /// and if so, return it.
        /// </summary>
        /// <returns>The resource definition.</returns>
        /// <typeparam name="TEntity">Target entity type</typeparam>
        public IResourceHookContainer<TEntity> GetResourceDefinition<TEntity>(ResourceHook hook = ResourceHook.None) where TEntity : class, IIdentifiable
        {
            return (IResourceHookContainer<TEntity>)GetResourceDefinition(typeof(TEntity), hook);
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

            _hookInTreeTraversal = _hookInTreeTraversal !=
                                        ResourceHook.None ?
                                        _hookInTreeTraversal :
                                        hook;
            foreach (Type targetType in nextLayerTypes)
            {
                var contextEntity = _graph.GetContextEntity(targetType);
                var relationshipsForContextEntity = contextEntity.Relationships.ToDictionary(
                                        attr => CreateMetaKey(attr, targetType, checkForDuplicates: true),
                                        attr => attr);
                /// keep only the meta info we really need for the traversal of the next layer
                /// also remove duplicates.
                PruneMetaDictionary(relationshipsForContextEntity, _hookInTreeTraversal);
                _meta = _meta.Concat(relationshipsForContextEntity)
                            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            return _meta;
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
            string newKey = $"{parentType.Name} {relationType} {attr.Type.Name}";
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
            var dupes = meta.Where(pair => pair.Key.Contains("DUPLICATE")).Select(pair => pair.Key);
            foreach (string target in dupes)
            {
                meta.Remove(target);
            }
            var noHookImplementation = meta.Where(pair => GetResourceDefinition(pair.Value.Type, targetHook) == null).Select(pair => pair.Key);
            foreach (string target in noHookImplementation)
            {
                meta.Remove(target);
            }
        }


    }
}