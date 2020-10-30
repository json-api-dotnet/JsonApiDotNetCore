using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Hooks.Internal
{
    /// <summary>
    /// Facade for hooks that does nothing, which is used when <see cref="IJsonApiOptions.EnableResourceHooks"/> is false.
    /// </summary>
    public sealed class NeverResourceHookExecutorFacade : IResourceHookExecutorFacade
    {
        public void BeforeReadSingle<TResource, TId>(TId resourceId, ResourcePipeline pipeline)
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

        public Task BeforeUpdateRelationshipAsync<TResource, TId>(TId id, Func<Task<TResource>> getResourceAsync)
            where TResource : class, IIdentifiable<TId>
        {
            return Task.CompletedTask;
        }

        public Task AfterUpdateRelationshipAsync<TResource, TId>(TId id, Func<Task<TResource>> getResourceAsync)
            where TResource : class, IIdentifiable<TId>
        {
            return Task.CompletedTask;
        }

        public Task BeforeDeleteAsync<TResource, TId>(TId id, Func<Task<TResource>> getResourceAsync)
            where TResource : class, IIdentifiable<TId>
        {
            return Task.CompletedTask;
        }

        public Task AfterDeleteAsync<TResource, TId>(TId id, Func<Task<TResource>> getResourceAsync)
            where TResource : class, IIdentifiable<TId>
        {
            return Task.CompletedTask;
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
