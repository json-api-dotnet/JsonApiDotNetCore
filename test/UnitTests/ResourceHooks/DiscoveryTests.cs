using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Hooks;
using System.Collections.Generic;
using Xunit;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;

namespace UnitTests.ResourceHooks
{
    public class DiscoveryTests
    {
        public class Dummy : Identifiable { }
        public class DummyResourceDefinition : ResourceDefinition<Dummy>
        {
            public DummyResourceDefinition() : base(new ResourceGraphBuilder().AddResource<Dummy>().Build()) { }

            public override IEnumerable<Dummy> BeforeDelete(HashSet<Dummy> entities, ResourcePipeline pipeline) { return entities; }
            public override void AfterDelete(HashSet<Dummy> entities, ResourcePipeline pipeline, bool succeeded) { }
        }

        [Fact]
        public void Hook_Discovery()
        {
            // arrange & act
            var hookConfig = new HooksDiscovery<Dummy>();
            // assert
            Assert.Contains(ResourceHook.BeforeDelete, hookConfig.ImplementedHooks);
            Assert.Contains(ResourceHook.AfterDelete, hookConfig.ImplementedHooks);

        }


        public class AnotherDummy : Identifiable { }
        public abstract class ResourceDefintionBase<T> : ResourceDefinition<T> where T : class, IIdentifiable
        {
            protected ResourceDefintionBase(IResourceGraph graph) : base(graph) { }
        }

        public class AnotherDummyResourceDefinition : ResourceDefintionBase<AnotherDummy>
        {
            public AnotherDummyResourceDefinition() : base(new ResourceGraphBuilder().AddResource<Dummy>().Build()) { }

            public override IEnumerable<AnotherDummy> BeforeDelete(HashSet<AnotherDummy> entities, ResourcePipeline pipeline) { return entities; }
            public override void AfterDelete(HashSet<AnotherDummy> entities, ResourcePipeline pipeline, bool succeeded) { }
        }
        [Fact]
        public void Hook_Discovery_With_Inheritance()
        {
            // arrange & act
            var hookConfig = new HooksDiscovery<AnotherDummy>();
            // assert
            Assert.Contains(ResourceHook.BeforeDelete, hookConfig.ImplementedHooks);
            Assert.Contains(ResourceHook.AfterDelete, hookConfig.ImplementedHooks);
        }
    }
}
