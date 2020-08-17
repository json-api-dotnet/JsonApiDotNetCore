using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Hooks;
using System.Collections.Generic;
using Xunit;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using System;
using JsonApiDotNetCore.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTests.ResourceHooks
{
    public sealed class DiscoveryTests
    {
        public class Dummy : Identifiable { }
        public sealed class DummyResourceDefinition : ResourceDefinition<Dummy>
        {
            public DummyResourceDefinition() : base(new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance).AddResource<Dummy>().Build()) { }

            public override IEnumerable<Dummy> BeforeDelete(IResourceHashSet<Dummy> affected, ResourcePipeline pipeline) { return affected; }
            public override void AfterDelete(HashSet<Dummy> resources, ResourcePipeline pipeline, bool succeeded) { }
        }

        private IServiceProvider MockProvider<TResource>(object service) where TResource : class, IIdentifiable
        {
            var services = new ServiceCollection();
            services.AddScoped((_) => (ResourceDefinition<TResource>)service);
            return services.BuildServiceProvider();
        }

        [Fact]
        public void HookDiscovery_StandardResourceDefinition_CanDiscover()
        {
            // Arrange & act
            var hookConfig = new HooksDiscovery<Dummy>(MockProvider<Dummy>(new DummyResourceDefinition()));
            // Assert
            Assert.Contains(ResourceHook.BeforeDelete, hookConfig.ImplementedHooks);
            Assert.Contains(ResourceHook.AfterDelete, hookConfig.ImplementedHooks);
        }

        public class AnotherDummy : Identifiable { }
        public abstract class ResourceDefinitionBase<T> : ResourceDefinition<T> where T : class, IIdentifiable
        {
            protected ResourceDefinitionBase(IResourceGraph resourceGraph) : base(resourceGraph) { }
            public override IEnumerable<T> BeforeDelete(IResourceHashSet<T> resources, ResourcePipeline pipeline) { return resources; }
            public override void AfterDelete(HashSet<T> resources, ResourcePipeline pipeline, bool succeeded) { }
        }

        public sealed class AnotherDummyResourceDefinition : ResourceDefinitionBase<AnotherDummy>
        {
            public AnotherDummyResourceDefinition() : base(new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance).AddResource<AnotherDummy>().Build()) { }
        }

        [Fact]
        public void HookDiscovery_InheritanceSubclass_CanDiscover()
        {
            // Arrange & act
            var hookConfig = new HooksDiscovery<AnotherDummy>(MockProvider<AnotherDummy>(new AnotherDummyResourceDefinition()));
            // Assert
            Assert.Contains(ResourceHook.BeforeDelete, hookConfig.ImplementedHooks);
            Assert.Contains(ResourceHook.AfterDelete, hookConfig.ImplementedHooks);
        }

        public class YetAnotherDummy : Identifiable { }
        public sealed class YetAnotherDummyResourceDefinition : ResourceDefinition<YetAnotherDummy>
        {
            public YetAnotherDummyResourceDefinition() : base(new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance).AddResource<YetAnotherDummy>().Build()) { }

            public override IEnumerable<YetAnotherDummy> BeforeDelete(IResourceHashSet<YetAnotherDummy> affected, ResourcePipeline pipeline) { return affected; }

            [LoadDatabaseValues(false)]
            public override void AfterDelete(HashSet<YetAnotherDummy> resources, ResourcePipeline pipeline, bool succeeded) { }
        }

        [Fact]
        public void HookDiscovery_WronglyUsedLoadDatabaseValueAttribute_ThrowsJsonApiSetupException()
        {
            //  assert
            Assert.Throws<JsonApiSetupException>(() =>
            {
                // Arrange & act
                new HooksDiscovery<YetAnotherDummy>(MockProvider<YetAnotherDummy>(new YetAnotherDummyResourceDefinition()));
            });
        }

        [Fact]
        public void HookDiscovery_InheritanceWithGenericSubclass_CanDiscover()
        {
            // Arrange & act
            var hookConfig = new HooksDiscovery<AnotherDummy>(MockProvider<AnotherDummy>(new GenericDummyResourceDefinition<AnotherDummy>()));

            // Assert
            Assert.Contains(ResourceHook.BeforeDelete, hookConfig.ImplementedHooks);
            Assert.Contains(ResourceHook.AfterDelete, hookConfig.ImplementedHooks);
        }

        public sealed class GenericDummyResourceDefinition<TResource> : ResourceDefinition<TResource> where TResource : class, IIdentifiable<int>
        {
            public GenericDummyResourceDefinition() : base(new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance).AddResource<TResource>().Build()) { }

            public override IEnumerable<TResource> BeforeDelete(IResourceHashSet<TResource> resources, ResourcePipeline pipeline) { return resources; }
            public override void AfterDelete(HashSet<TResource> resources, ResourcePipeline pipeline, bool succeeded) { }
        }
    }
}
