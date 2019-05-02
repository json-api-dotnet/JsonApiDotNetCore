using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Internal
{
    /// <summary>
    /// A helper class that retrieves all metadata required for the hook 
    /// executor to call resource hooks.  It gets RelationshipAttributes, 
    /// ResourceHookContainers and figures out wether hooks are actually implemented.
    /// </summary>
    public interface IHookExecutorHelper
    {
        /// <summary>
        /// Determines if the implemented <param name="hook"/> requires the 
        /// EntityDiff parameter (in the hook execution) to have a populated
        /// DatabaseEntities field.
        /// </summary>
        /// <returns><c>true</c>, if enabled<c>false</c> otherwise.</returns>
        /// <param name="container">Container.</param>
        /// <param name="hook">Hook.</param>
        bool RequiresDatabaseDiff(IResourceHookContainer container, ResourceHook hook);

        /// <summary>
        /// Retrieves all the RelationshipProxies for a given entity. This method 
        /// is used by the HookExecutor when looping through the entities in a layer
        /// of the breadth first traversal.
        /// </summary>
        /// <returns>The relationship proxies related to the particular entity</returns>
        /// <param name="entity">The entity of intrest in breadth first traversal</param>
        IEnumerable<RelationshipProxy> GetMetaEntries(IIdentifiable entity);

        /// <summary>
        /// For a particular ResourceHook and for a given model type, checks if 
        /// the ResourceDefinition has an implementation for the hook
        /// and if so, return it.
        /// 
        /// Also caches the retrieves containers so we don't need to reflectively
        /// instantiate them multiple times.
        /// </summary>
        /// <returns>The resource definition.</returns>
        /// <param name="targetEntity">Target entity type</param>
        /// <param name="hook">The hook to get a ResourceDefinition for.</param>
        IResourceHookContainer GetResourceHookContainer(Type targetEntity, ResourceHook hook = ResourceHook.None);

        /// <summary>
        /// For a particular ResourceHook and for a given model type, checks if 
        /// the ResourceDefinition has an implementation for the hook
        /// and if so, return it.
        /// 
        /// Also caches the retrieves containers so we don't need to reflectively
        /// instantiate them multiple times.
        /// </summary>
        /// <returns>The resource definition.</returns>
        /// <typeparam name="TEntity">Target entity type</typeparam>
        /// <param name="hook">The hook to get a ResourceDefinition for.</param>
        IResourceHookContainer<TEntity> GetResourceHookContainer<TEntity>(ResourceHook hook = ResourceHook.None) where TEntity : class, IIdentifiable;

        /// <summary>
        /// Checks if the given hook is implemented on the given container
        /// </summary>
        /// <returns><c>true</c>, if the hook was implemented, <c>false</c> otherwise.</returns>
        /// <param name="container">Container.</param>
        /// <param name="hook">Hook.</param>
        bool ShouldExecuteHook(Type targetType, ResourceHook hook);

        /// <summary>
        /// For the types in <paramref name="nextEntityTreeLayerTypes"/>, given (a set of)
        /// <paramref name="hook"/>s,  retrieves the relationships from 
        /// ContextEntity and gets the resource definitions (hook containers) for
        /// these relationships. This information if required for the breadth first
        /// traversal of the next layer.
        /// </summary>
        /// <returns>The meta dict.</returns>
        /// <param name="nextEntityTreeLayerTypes">Unique list of types to extract metadata from</param>
        /// <param name="hook">The target resource hook types</param>
        Dictionary<Type, List<RelationshipProxy>> UpdateMetaInformation(IEnumerable<Type> nextEntityTreeLayerTypes, ResourceHook hook = ResourceHook.None);
        /// <summary>
        /// For the types in <paramref name="nextEntityTreeLayerTypes"/>, given (a set of)
        /// <paramref name="hooks"/>s,  retrieves the relationships from 
        /// ContextEntity and gets the resource definitions (hook containers) for
        /// these relationships. This information if required for the breadth first
        /// traversal of the next layer.
        /// </summary>
        /// <returns>The meta dict.</returns>
        /// <param name="nextEntityTreeLayerTypes">Unique list of types to extract metadata from</param>
        /// <param name="hooks">The target resource hook types</param>
        Dictionary<Type, List<RelationshipProxy>> UpdateMetaInformation(IEnumerable<Type> nextEntityTreeLayerTypes, IEnumerable<ResourceHook> hooks);
    }
}