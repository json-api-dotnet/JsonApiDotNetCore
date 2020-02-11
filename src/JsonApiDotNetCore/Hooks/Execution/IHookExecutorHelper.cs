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
    /// Used internally by <see cref="ResourceHookExecutor"/>
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
        IResourceHookContainer<TResource> GetResourceHookContainer<TResource>(ResourceHook hook = ResourceHook.None) where TResource : class, IIdentifiable;

        /// <summary>
        /// Load the implicitly affected entities from the database for a given set of target target entities and involved relationships
        /// </summary>
        /// <returns>The implicitly affected entities by relationship</returns>
        Dictionary<RelationshipAttribute, IEnumerable> LoadImplicitlyAffected(Dictionary<RelationshipAttribute, IEnumerable> leftEntities, IEnumerable existingRightEntities = null);

        /// <summary>
        /// For a set of entities, loads current values from the database
        /// </summary>
        /// <param name="repositoryEntityType">type of the entities to be loaded</param>
        /// <param name="entities">The set of entities to load the db values for</param>
        /// <param name="hook">The hook in which the db values will be displayed.</param>
        /// <param name="relationships">Relationships that need to be included on entities.</param>
        IEnumerable LoadDbValues(Type repositoryEntityType, IEnumerable entities, ResourceHook hook, params RelationshipAttribute[] relationships);

        /// <summary>
        /// Checks if the display database values option is allowed for the targeted hook, and for 
        /// a given resource of type <paramref name="entityType"/> checks if this hook is implemented and if the
        /// database values option is enabled.
        /// </summary>
        /// <returns><c>true</c>, if should load db values, <c>false</c> otherwise.</returns>
        /// <param name="entityType">Container entity type.</param>
        /// <param name="hook">Hook.</param>
        bool ShouldLoadDbValues(Type entityType, ResourceHook hook);
    }
}
