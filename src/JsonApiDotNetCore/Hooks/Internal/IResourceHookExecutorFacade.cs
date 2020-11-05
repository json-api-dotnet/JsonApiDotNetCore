using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Hooks.Internal
{
    /// <summary>
    /// Facade for execution of resource hooks.
    /// </summary>
    public interface IResourceHookExecutorFacade
    {
        void BeforeReadSingle<TResource, TId>(TId resourceId, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable<TId>;

        void AfterReadSingle<TResource>(TResource resource, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable;

        void BeforeReadMany<TResource>()
            where TResource : class, IIdentifiable;

        void AfterReadMany<TResource>(IReadOnlyCollection<TResource> resources)
            where TResource : class, IIdentifiable;

        void BeforeCreate<TResource>(TResource resource)
            where TResource : class, IIdentifiable;

        void AfterCreate<TResource>(TResource resource)
            where TResource : class, IIdentifiable;

        void BeforeUpdateResource<TResource>(TResource resource)
            where TResource : class, IIdentifiable;

        void AfterUpdateResource<TResource>(TResource resource)
            where TResource : class, IIdentifiable;

        void BeforeUpdateRelationshipAsync<TResource>(TResource resource)
            where TResource : class, IIdentifiable;

        void AfterUpdateRelationshipAsync<TResource>(TResource resource)
            where TResource : class, IIdentifiable;

        Task BeforeDeleteAsync<TResource, TId>(TId id, Func<Task<TResource>> getResourceAsync)
            where TResource : class, IIdentifiable<TId>;

        Task AfterDeleteAsync<TResource, TId>(TId id, Func<Task<TResource>> getResourceAsync)
            where TResource : class, IIdentifiable<TId>;

        void OnReturnSingle<TResource>(TResource resource, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable;

        IReadOnlyCollection<TResource> OnReturnMany<TResource>(IReadOnlyCollection<TResource> resources)
            where TResource : class, IIdentifiable;

        object OnReturnRelationship(object resourceOrResources);
    }
}
