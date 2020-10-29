using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Hooks.Internal
{
    /// <summary>
    /// Hooks implementation that does nothing, which is used when <see cref="IJsonApiOptions.EnableResourceHooks"/> is false.
    /// </summary>
    public sealed class NeverResourceHookExecutor : IResourceHookExecutor
    {
        public void BeforeRead<TResource>(ResourcePipeline pipeline, string stringId = null)
            where TResource : class, IIdentifiable
        {
        }

        public void AfterRead<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable
        {
        }

        public IEnumerable<TResource> BeforeUpdate<TResource>(IEnumerable<TResource> resources,
            ResourcePipeline pipeline)
            where TResource : class, IIdentifiable
        {
            return resources;
        }

        public void AfterUpdate<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable
        {
        }

        public IEnumerable<TResource> BeforeCreate<TResource>(IEnumerable<TResource> resources,
            ResourcePipeline pipeline)
            where TResource : class, IIdentifiable
        {
            return resources;
        }

        public void AfterCreate<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable
        {
        }

        public IEnumerable<TResource> BeforeDelete<TResource>(IEnumerable<TResource> resources,
            ResourcePipeline pipeline)
            where TResource : class, IIdentifiable
        {
            return resources;
        }

        public void AfterDelete<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline, bool succeeded)
            where TResource : class, IIdentifiable
        {
        }

        public IEnumerable<TResource> OnReturn<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable
        {
            return resources;
        }
    }
}
