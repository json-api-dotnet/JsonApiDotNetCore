using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Hooks;
using System.Collections.Generic;
using Xunit;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using System;
using Moq;
using JsonApiDotNetCore.Services;

namespace UnitTests.ResourceHooks
{
    public class DiscoveryTests
    {
        public class Dummy : Identifiable { }
        public class DummyResourceDefinition : ResourceDefinition<Dummy>
        {
            public DummyResourceDefinition() : base(new ResourceGraphBuilder().AddResource<Dummy>().Build()) { }

            public override IEnumerable<Dummy> BeforeDelete(IEntityHashSet<Dummy> affected, ResourcePipeline pipeline) { return affected; }
            public override void AfterDelete(HashSet<Dummy> entities, ResourcePipeline pipeline, bool succeeded) { }
        }

        private IServiceProvider MockProvider(object service)
        {
            var mock = new Mock<IServiceProvider>();
            mock.Setup(m => m.GetService(It.IsAny<Type>())).Returns(service);
            return mock.Object;
        }


        [Fact]
        public void Hook_Discovery()
        {
            // Arrange & act
            var hookConfig = new HooksDiscovery<Dummy>(MockProvider(new DummyResourceDefinition()));
            // Assert
            Assert.Contains(ResourceHook.BeforeDelete, hookConfig.ImplementedHooks);
            Assert.Contains(ResourceHook.AfterDelete, hookConfig.ImplementedHooks);

        }

        public class AnotherDummy : Identifiable { }
        public abstract class ResourceDefintionBase<T> : ResourceDefinition<T> where T : class, IIdentifiable
        {
            public ResourceDefintionBase(IResourceGraph resourceGraph) : base(resourceGraph)
            {
            }

            public override IEnumerable<T> BeforeDelete(IEntityHashSet<T> affected, ResourcePipeline pipeline) { return affected; }
            public override void AfterDelete(HashSet<T> entities, ResourcePipeline pipeline, bool succeeded) { }
        }

        public class AnotherDummyResourceDefinition : ResourceDefintionBase<AnotherDummy>
        {
            public AnotherDummyResourceDefinition() : base(new ResourceGraphBuilder().AddResource<AnotherDummy>().Build()) { }
        }
        [Fact]
        public void Hook_Discovery_With_Inheritance()
        {
            // Arrange & act
            var hookConfig = new HooksDiscovery<AnotherDummy>(MockProvider(new AnotherDummyResourceDefinition()));
            // Assert
            Assert.Contains(ResourceHook.BeforeDelete, hookConfig.ImplementedHooks);
            Assert.Contains(ResourceHook.AfterDelete, hookConfig.ImplementedHooks);
        }


        public class YetAnotherDummy : Identifiable { }
        public class YetAnotherDummyResourceDefinition : ResourceDefinition<YetAnotherDummy>
        {
            public YetAnotherDummyResourceDefinition() : base(new ResourceGraphBuilder().AddResource<YetAnotherDummy>().Build()) { }

            public override IEnumerable<YetAnotherDummy> BeforeDelete(IEntityHashSet<YetAnotherDummy> affected, ResourcePipeline pipeline) { return affected; }

            [LoadDatabaseValues(false)]
            public override void AfterDelete(HashSet<YetAnotherDummy> entities, ResourcePipeline pipeline, bool succeeded) { }
        }
        [Fact]
        public void LoadDatabaseValues_Attribute_Not_Allowed()
        {
            //  assert
            Assert.Throws<JsonApiSetupException>(() =>
            {
                // Arrange & act
                var hookConfig = new HooksDiscovery<YetAnotherDummy>(MockProvider(new YetAnotherDummyResourceDefinition()));
            });

        }
    }
}
