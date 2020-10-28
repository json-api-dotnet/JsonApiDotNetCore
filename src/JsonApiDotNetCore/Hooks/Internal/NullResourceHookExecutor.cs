using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Hooks.Internal
{
    internal sealed class NullResourceHookExecutor : IResourceHookExecutor
    {
        public void BeforeRead<TResource>(ResourcePipeline pipeline, string stringId = null) where TResource : class, IIdentifiable => throw new NotImplementedException();

        public void AfterRead<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline) where TResource : class, IIdentifiable => throw new NotImplementedException();

        public IEnumerable<TResource> BeforeUpdate<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline) where TResource : class, IIdentifiable => throw new NotImplementedException();

        public void AfterUpdate<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline) where TResource : class, IIdentifiable => throw new NotImplementedException();

        public IEnumerable<TResource> BeforeCreate<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline) where TResource : class, IIdentifiable => throw new NotImplementedException();

        public void AfterCreate<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline) where TResource : class, IIdentifiable => throw new NotImplementedException();

        public IEnumerable<TResource> BeforeDelete<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline) where TResource : class, IIdentifiable => throw new NotImplementedException();

        public void AfterDelete<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline, bool succeeded) where TResource : class, IIdentifiable => throw new NotImplementedException();

        public IEnumerable<TResource> OnReturn<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline) where TResource : class, IIdentifiable => throw new NotImplementedException();
    }
}
