using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Hooks.Internal
{
    /// <summary>
    /// Facade for hooks that invokes callbacks on <see cref="ResourceHooksDefinition{TResource}" />, which is used when
    /// <see cref="IJsonApiOptions.EnableResourceHooks" /> is true.
    /// </summary>
    internal sealed class ResourceHookExecutorFacade : IResourceHookExecutorFacade
    {
        private readonly IResourceHookExecutor _resourceHookExecutor;
        private readonly IResourceFactory _resourceFactory;

        public ResourceHookExecutorFacade(IResourceHookExecutor resourceHookExecutor, IResourceFactory resourceFactory)
        {
            ArgumentGuard.NotNull(resourceHookExecutor, nameof(resourceHookExecutor));
            ArgumentGuard.NotNull(resourceFactory, nameof(resourceFactory));

            _resourceHookExecutor = resourceHookExecutor;
            _resourceFactory = resourceFactory;
        }

        public void BeforeReadSingle<TResource, TId>(TId id, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable<TId>
        {
            var temporaryResource = _resourceFactory.CreateInstance<TResource>();
            temporaryResource.Id = id;

            _resourceHookExecutor.BeforeRead<TResource>(pipeline, temporaryResource.StringId);
        }

        public void AfterReadSingle<TResource>(TResource resource, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable
        {
            _resourceHookExecutor.AfterRead(resource.AsList(), pipeline);
        }

        public void BeforeReadMany<TResource>()
            where TResource : class, IIdentifiable
        {
            _resourceHookExecutor.BeforeRead<TResource>(ResourcePipeline.Get);
        }

        public void AfterReadMany<TResource>(IReadOnlyCollection<TResource> resources)
            where TResource : class, IIdentifiable
        {
            _resourceHookExecutor.AfterRead(resources, ResourcePipeline.Get);
        }

        public void BeforeCreate<TResource>(TResource resource)
            where TResource : class, IIdentifiable
        {
            _resourceHookExecutor.BeforeCreate(resource.AsList(), ResourcePipeline.Post);
        }

        public void AfterCreate<TResource>(TResource resource)
            where TResource : class, IIdentifiable
        {
            _resourceHookExecutor.AfterCreate(resource.AsList(), ResourcePipeline.Post);
        }

        public void BeforeUpdateResource<TResource>(TResource resource)
            where TResource : class, IIdentifiable
        {
            _resourceHookExecutor.BeforeUpdate(resource.AsList(), ResourcePipeline.Patch);
        }

        public void AfterUpdateResource<TResource>(TResource resource)
            where TResource : class, IIdentifiable
        {
            _resourceHookExecutor.AfterUpdate(resource.AsList(), ResourcePipeline.Patch);
        }

        public void BeforeUpdateRelationship<TResource>(TResource resource)
            where TResource : class, IIdentifiable
        {
            _resourceHookExecutor.BeforeUpdate(resource.AsList(), ResourcePipeline.PatchRelationship);
        }

        public void AfterUpdateRelationship<TResource>(TResource resource)
            where TResource : class, IIdentifiable
        {
            _resourceHookExecutor.AfterUpdate(resource.AsList(), ResourcePipeline.PatchRelationship);
        }

        public void BeforeDelete<TResource, TId>(TId id)
            where TResource : class, IIdentifiable<TId>
        {
            var temporaryResource = _resourceFactory.CreateInstance<TResource>();
            temporaryResource.Id = id;

            _resourceHookExecutor.BeforeDelete(temporaryResource.AsList(), ResourcePipeline.Delete);
        }

        public void AfterDelete<TResource, TId>(TId id)
            where TResource : class, IIdentifiable<TId>
        {
            var temporaryResource = _resourceFactory.CreateInstance<TResource>();
            temporaryResource.Id = id;

            _resourceHookExecutor.AfterDelete(temporaryResource.AsList(), ResourcePipeline.Delete, true);
        }

        public void OnReturnSingle<TResource>(TResource resource, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable
        {
            _resourceHookExecutor.OnReturn(resource.AsList(), pipeline);
        }

        public IReadOnlyCollection<TResource> OnReturnMany<TResource>(IReadOnlyCollection<TResource> resources)
            where TResource : class, IIdentifiable
        {
            return _resourceHookExecutor.OnReturn(resources, ResourcePipeline.Get).ToArray();
        }

        public object OnReturnRelationship(object resourceOrResources)
        {
            if (resourceOrResources is IEnumerable)
            {
                dynamic resources = resourceOrResources;
                return Enumerable.ToArray(_resourceHookExecutor.OnReturn(resources, ResourcePipeline.GetRelationship));
            }

            if (resourceOrResources is IIdentifiable)
            {
                dynamic resources = ObjectExtensions.AsList((dynamic)resourceOrResources);
                return Enumerable.SingleOrDefault(_resourceHookExecutor.OnReturn(resources, ResourcePipeline.GetRelationship));
            }

            return resourceOrResources;
        }
    }
}
