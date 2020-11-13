using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Hooks.Internal
{
    /// <summary>
    /// Facade for hooks that never executes any callbacks, which is used when <see cref="IJsonApiOptions.EnableResourceHooks"/> is false.
    /// </summary>
    public sealed class NeverResourceHookExecutorFacade : IResourceHookExecutorFacade
    {
        public void BeforeReadSingle<TResource, TId>(TId id, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable<TId>
        {
        }

        public void AfterReadSingle<TResource>(TResource resource, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable
        {
        }

        public void BeforeReadMany<TResource>()
            where TResource : class, IIdentifiable
        {
        }

        public void AfterReadMany<TResource>(IReadOnlyCollection<TResource> resources)
            where TResource : class, IIdentifiable
        {
        }

        public void BeforeCreate<TResource>(TResource resource)
            where TResource : class, IIdentifiable
        {
        }

        public void AfterCreate<TResource>(TResource resource)
            where TResource : class, IIdentifiable
        {
        }

        public void BeforeUpdateResource<TResource>(TResource resource)
            where TResource : class, IIdentifiable
        {
        }

        public void AfterUpdateResource<TResource>(TResource resource)
            where TResource : class, IIdentifiable
        {
        }

        public void BeforeUpdateRelationship<TResource>(TResource resource)
            where TResource : class, IIdentifiable
        {
        }

        public void AfterUpdateRelationship<TResource>(TResource resource)
            where TResource : class, IIdentifiable
        {
        }

        public void BeforeDelete<TResource, TId>(TId id)
            where TResource : class, IIdentifiable<TId>
        {
        }

        public void AfterDelete<TResource, TId>(TId id)
            where TResource : class, IIdentifiable<TId>
        {
        }

        public void OnReturnSingle<TResource>(TResource resource, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable
        {
        }

        public IReadOnlyCollection<TResource> OnReturnMany<TResource>(IReadOnlyCollection<TResource> resources)
            where TResource : class, IIdentifiable
        {
            return resources;
        }

        public object OnReturnRelationship(object resourceOrResources)
        {
            return resourceOrResources;
        }
    }
}
