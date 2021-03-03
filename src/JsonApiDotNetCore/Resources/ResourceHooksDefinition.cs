using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Hooks.Internal;
using JsonApiDotNetCore.Hooks.Internal.Execution;

namespace JsonApiDotNetCore.Resources
{
    /// <summary>
    /// Provides a resource-specific extensibility point for API developers to be notified of various events and influence behavior using custom code. It is
    /// intended to improve the developer experience and reduce boilerplate for commonly required features. The goal of this class is to reduce the frequency
    /// with which developers have to override the service and repository layers.
    /// </summary>
    /// <typeparam name="TResource">
    /// The resource type.
    /// </typeparam>
    [PublicAPI]
    public class ResourceHooksDefinition<TResource> : IResourceHookContainer<TResource>
        where TResource : class, IIdentifiable
    {
        protected IResourceGraph ResourceGraph { get; }

        public ResourceHooksDefinition(IResourceGraph resourceGraph)
        {
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));

            ResourceGraph = resourceGraph;
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method is part of Resource Hooks, which is an experimental feature and subject to change in future versions.
        /// </remarks>
        public virtual void AfterCreate(HashSet<TResource> resources, ResourcePipeline pipeline)
        {
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method is part of Resource Hooks, which is an experimental feature and subject to change in future versions.
        /// </remarks>
        public virtual void AfterRead(HashSet<TResource> resources, ResourcePipeline pipeline, bool isIncluded = false)
        {
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method is part of Resource Hooks, which is an experimental feature and subject to change in future versions.
        /// </remarks>
        public virtual void AfterUpdate(HashSet<TResource> resources, ResourcePipeline pipeline)
        {
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method is part of Resource Hooks, which is an experimental feature and subject to change in future versions.
        /// </remarks>
        public virtual void AfterDelete(HashSet<TResource> resources, ResourcePipeline pipeline, bool succeeded)
        {
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method is part of Resource Hooks, which is an experimental feature and subject to change in future versions.
        /// </remarks>
        public virtual void AfterUpdateRelationship(IRelationshipsDictionary<TResource> resourcesByRelationship, ResourcePipeline pipeline)
        {
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method is part of Resource Hooks, which is an experimental feature and subject to change in future versions.
        /// </remarks>
        public virtual IEnumerable<TResource> BeforeCreate(IResourceHashSet<TResource> resources, ResourcePipeline pipeline)
        {
            return resources;
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method is part of Resource Hooks, which is an experimental feature and subject to change in future versions.
        /// </remarks>
        public virtual void BeforeRead(ResourcePipeline pipeline, bool isIncluded = false, string stringId = null)
        {
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method is part of Resource Hooks, which is an experimental feature and subject to change in future versions.
        /// </remarks>
        public virtual IEnumerable<TResource> BeforeUpdate(IDiffableResourceHashSet<TResource> resources, ResourcePipeline pipeline)
        {
            return resources;
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method is part of Resource Hooks, which is an experimental feature and subject to change in future versions.
        /// </remarks>
        public virtual IEnumerable<TResource> BeforeDelete(IResourceHashSet<TResource> resources, ResourcePipeline pipeline)
        {
            return resources;
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method is part of Resource Hooks, which is an experimental feature and subject to change in future versions.
        /// </remarks>
        public virtual IEnumerable<string> BeforeUpdateRelationship(HashSet<string> ids, IRelationshipsDictionary<TResource> resourcesByRelationship,
            ResourcePipeline pipeline)
        {
            return ids;
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method is part of Resource Hooks, which is an experimental feature and subject to change in future versions.
        /// </remarks>
        public virtual void BeforeImplicitUpdateRelationship(IRelationshipsDictionary<TResource> resourcesByRelationship, ResourcePipeline pipeline)
        {
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method is part of Resource Hooks, which is an experimental feature and subject to change in future versions.
        /// </remarks>
        public virtual IEnumerable<TResource> OnReturn(HashSet<TResource> resources, ResourcePipeline pipeline)
        {
            return resources;
        }
    }
}
