using System.Collections.Generic;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Hooks
{
    /// <summary>
    /// Facade for execution of resource hooks.
    /// </summary>
    public interface IResourceHookExecutorFacade
    {
        void BeforeReadSingle<TResource, TId>(TId id, ResourcePipeline pipeline)
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

        void BeforeUpdateRelationship<TResource>(TResource resource)
            where TResource : class, IIdentifiable;

        void AfterUpdateRelationship<TResource>(TResource resource)
            where TResource : class, IIdentifiable;

        void BeforeDelete<TResource, TId>(TId id)
            where TResource : class, IIdentifiable<TId>;

        void AfterDelete<TResource, TId>(TId id)
            where TResource : class, IIdentifiable<TId>;

        void OnReturnSingle<TResource>(TResource resource, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable;

        IReadOnlyCollection<TResource> OnReturnMany<TResource>(IReadOnlyCollection<TResource> resources)
            where TResource : class, IIdentifiable;

        object OnReturnRelationship(object resourceOrResources);
    }
}
