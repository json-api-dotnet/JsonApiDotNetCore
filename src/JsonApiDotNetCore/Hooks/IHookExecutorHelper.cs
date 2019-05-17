using System;
using System.Collections;
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
    internal interface IHookExecutorHelper
    {
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

        Dictionary<RelationshipProxy, IEnumerable> LoadImplicitlyAffected(Dictionary<RelationshipProxy, IEnumerable> principalEntities, IEnumerable existingDependentEntities = null);

        IEnumerable LoadDbValues(Type entityType, IEnumerable entities, ResourceHook hook, params RelationshipProxy[] relationships);
        HashSet<TEntity> LoadDbValues<TEntity>(IEnumerable<TEntity> entities, ResourceHook hook, params RelationshipProxy[] relationships) where TEntity : class, IIdentifiable;
    }
}