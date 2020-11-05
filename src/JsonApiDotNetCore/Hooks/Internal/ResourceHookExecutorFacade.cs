using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public void BeforeReadSingle<TResource, TId>(TId resourceId, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable<TId>
        {
            var temporaryResource = _resourceFactory.CreateInstance<TResource>();
            temporaryResource.Id = resourceId;

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

        public void BeforeUpdateRelationshipAsync<TResource>(TResource resource)
            where TResource : class, IIdentifiable
        {
            _resourceHookExecutor.BeforeUpdate(ToList(resource), ResourcePipeline.PatchRelationship);
        }

        public void AfterUpdateRelationshipAsync<TResource>(TResource resource)
            where TResource : class, IIdentifiable
        {
            _resourceHookExecutor.AfterUpdate(ToList(resource), ResourcePipeline.PatchRelationship);
        }

        public async Task BeforeDeleteAsync<TResource, TId>(TId id, Func<Task<TResource>> getResourceAsync)
            where TResource : class, IIdentifiable<TId>
        {
            var resource = await getResourceAsync();
            _resourceHookExecutor.BeforeDelete(ToList(resource), ResourcePipeline.Delete);
        }

        public async Task AfterDeleteAsync<TResource, TId>(TId id, Func<Task<TResource>> getResourceAsync)
            where TResource : class, IIdentifiable<TId>
        {
            var resource = await getResourceAsync();
            _resourceHookExecutor.AfterDelete(ToList(resource), ResourcePipeline.Delete, true);
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
            if (resourceOrResources is IEnumerable enumerable)
            {
                var resources = enumerable.Cast<IIdentifiable>();
                return _resourceHookExecutor.OnReturn(resources, ResourcePipeline.GetRelationship).ToArray();
            }

            if (resourceOrResources is IIdentifiable identifiable)
            {
                var resources = ToList(identifiable);
                return _resourceHookExecutor.OnReturn(resources, ResourcePipeline.GetRelationship).Single();
            }

            return resourceOrResources;
        }

        private static List<TResource> ToList<TResource>(TResource resource)
        {
            return new List<TResource> {resource};
        }
    }
}
