using System;
using System.Collections;
using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Hooks
{
    /// <summary>
    /// A helper class for retrieving meta data about hooks, 
    /// fetching database values and performing other recurring internal operations.
    /// 
    /// Used internalyl by <see cref="ResourceHookExecutor"/>
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
        IResourceHookContainer GetResourceHookContainer(Type targetEntity, ResourceHook hook = ResourceHook.None);

        /// <summary>
        /// For a particular ResourceHook and for a given model type, checks if 
        /// the ResourceDefinition has an implementation for the hook
        /// and if so, return it.
        /// 
        /// Also caches the retrieves containers so we don't need to reflectively
        /// instantiate them multiple times.
        /// </summary>
        IResourceHookContainer<TEntity> GetResourceHookContainer<TEntity>(ResourceHook hook = ResourceHook.None) where TEntity : class, IIdentifiable;

        /// <summary>
        /// Load the implicitly affected entities from the database for a given set of target target entities and involved relationships
        /// </summary>
        /// <returns>The implicitly affected entities by relationship</returns>
        Dictionary<RelationshipProxy, IEnumerable> LoadImplicitlyAffected(Dictionary<RelationshipProxy, IEnumerable> principalEntities, IEnumerable existingDependentEntities = null);

        /// <summary>
        /// For a set of entities, loads current values from the database
        /// </summary>
        IEnumerable LoadDbValues(Type repositoryEntityType, Type affectedHookEntityType,  IEnumerable entities, ResourceHook hook, params RelationshipProxy[] relationships);
        /// <summary>
        /// For a set of entities, loads current values from the database
        /// </summary>
        HashSet<TEntity> LoadDbValues<TEntity>(IEnumerable<TEntity> entities, ResourceHook hook, params RelationshipProxy[] relationships) where TEntity : class, IIdentifiable;
    }
}