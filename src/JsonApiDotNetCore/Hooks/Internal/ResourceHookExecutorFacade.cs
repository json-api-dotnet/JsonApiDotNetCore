using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Hooks.Internal
{
    /// <summary>
    /// Facade for hooks that invokes callbacks on <see cref="ResourceHooksDefinition{TResource}"/>,
    /// which is used when <see cref="IJsonApiOptions.EnableResourceHooks"/> is true.
    /// </summary>
    internal sealed class ResourceHookExecutorFacade : IResourceHookExecutorFacade
    {
        private readonly IResourceHookExecutor _resourceHookExecutor;
        private readonly IResourceFactory _resourceFactory;

        public ResourceHookExecutorFacade(IResourceHookExecutor resourceHookExecutor, IResourceFactory resourceFactory)
        {
            _resourceHookExecutor =
                resourceHookExecutor ?? throw new ArgumentNullException(nameof(resourceHookExecutor));
            _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
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
            _resourceHookExecutor.AfterRead(ToList(resource), pipeline);
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
            _resourceHookExecutor.BeforeCreate(ToList(resource), ResourcePipeline.Post);
        }

        public void AfterCreate<TResource>(TResource resource)
            where TResource : class, IIdentifiable
        {
            _resourceHookExecutor.AfterCreate(ToList(resource), ResourcePipeline.Post);
        }

        public void BeforeUpdateResource<TResource>(TResource resource)
            where TResource : class, IIdentifiable
        {
            _resourceHookExecutor.BeforeUpdate(ToList(resource), ResourcePipeline.Patch);
        }

        public void AfterUpdateResource<TResource>(TResource resource)
            where TResource : class, IIdentifiable
        {
            _resourceHookExecutor.AfterUpdate(ToList(resource), ResourcePipeline.Patch);
        }

        public void BeforeUpdateRelationship<TResource>(TResource resource)
            where TResource : class, IIdentifiable
        {
            _resourceHookExecutor.BeforeUpdate(ToList(resource), ResourcePipeline.PatchRelationship);
        }

        public void AfterUpdateRelationship<TResource>(TResource resource)
            where TResource : class, IIdentifiable
        {
            _resourceHookExecutor.AfterUpdate(ToList(resource), ResourcePipeline.PatchRelationship);
        }

        public void BeforeDelete<TResource, TId>(TId id)
            where TResource : class, IIdentifiable<TId>
        {
            var temporaryResource = _resourceFactory.CreateInstance<TResource>();
            temporaryResource.Id = id;

            _resourceHookExecutor.BeforeDelete(ToList(temporaryResource), ResourcePipeline.Delete);
        }

        public void AfterDelete<TResource, TId>(TId id)
            where TResource : class, IIdentifiable<TId>
        {
            var temporaryResource = _resourceFactory.CreateInstance<TResource>();
            temporaryResource.Id = id;

            _resourceHookExecutor.AfterDelete(ToList(temporaryResource), ResourcePipeline.Delete, true);
        }

        public void OnReturnSingle<TResource>(TResource resource, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable
        {
            _resourceHookExecutor.OnReturn(ToList(resource), pipeline);
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
                var resources = ToList((dynamic)resourceOrResources);
                return Enumerable.SingleOrDefault(_resourceHookExecutor.OnReturn(resources, ResourcePipeline.GetRelationship));
            }

            return resourceOrResources;
        }

        private static List<TResource> ToList<TResource>(TResource resource)
        {
            return new List<TResource> {resource};
        }
    }
}
