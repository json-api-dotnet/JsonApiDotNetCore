using System;
using System.Collections;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Hooks.Internal.Execution
{
    /// <summary>
    /// A helper class for retrieving meta data about hooks, fetching database values and performing other recurring internal operations. Used internally by
    /// <see cref="ResourceHookExecutor" />
    /// </summary>
    internal interface IHookContainerProvider
    {
        /// <summary>
        /// For a particular ResourceHook and for a given model type, checks if the ResourceHooksDefinition has an implementation for the hook and if so, return
        /// it. Also caches the retrieves containers so we don't need to reflectively instantiate them multiple times.
        /// </summary>
        IResourceHookContainer GetResourceHookContainer(Type targetResource, ResourceHook hook = ResourceHook.None);

        /// <summary>
        /// For a particular ResourceHook and for a given model type, checks if the ResourceHooksDefinition has an implementation for the hook and if so, return
        /// it. Also caches the retrieves containers so we don't need to reflectively instantiate them multiple times.
        /// </summary>
        IResourceHookContainer<TResource> GetResourceHookContainer<TResource>(ResourceHook hook = ResourceHook.None)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Load the implicitly affected resources from the database for a given set of target target resources and involved relationships
        /// </summary>
        /// <returns>
        /// The implicitly affected resources by relationship
        /// </returns>
        IDictionary<RelationshipAttribute, IEnumerable> LoadImplicitlyAffected(IDictionary<RelationshipAttribute, IEnumerable> leftResourcesByRelation,
            IEnumerable existingRightResources = null);

        /// <summary>
        /// For a set of resources, loads current values from the database
        /// </summary>
        /// <param name="resourceTypeForRepository">
        /// type of the resources to be loaded
        /// </param>
        /// <param name="resources">
        /// The set of resources to load the db values for
        /// </param>
        /// <param name="relationships">
        /// Relationships that need to be included on resources.
        /// </param>
        IEnumerable LoadDbValues(Type resourceTypeForRepository, IEnumerable resources, params RelationshipAttribute[] relationships);

        /// <summary>
        /// Checks if the display database values option is allowed for the targeted hook, and for a given resource of type <paramref name="resourceType" />
        /// checks if this hook is implemented and if the database values option is enabled.
        /// </summary>
        /// <returns>
        /// <c>true</c>, if should load db values, <c>false</c> otherwise.
        /// </returns>
        /// <param name="resourceType">
        /// Container resource type.
        /// </param>
        /// <param name="hook">Hook.</param>
        bool ShouldLoadDbValues(Type resourceType, ResourceHook hook);
    }
}
