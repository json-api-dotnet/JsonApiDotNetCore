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

            public override IEnumerable<Dummy> BeforeDelete(IEntityHashSet<Dummy> affected, ResourcePipeline pipeline) { return affected; }
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

            public override IEnumerable<T> BeforeDelete(IEntityHashSet<T> affected, ResourcePipeline pipeline) { return affected; }
            public override void AfterDelete(HashSet<T> entities, ResourcePipeline pipeline, bool succeeded) { }
        }

        public class AnotherDummyResourceDefinition : ResourceDefintionBase<AnotherDummy>
        {
            public AnotherDummyResourceDefinition() : base(new ResourceGraphBuilder().AddResource<Dummy>().Build()) { }
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
                // arrange & act
                var hookConfig = new HooksDiscovery<YetAnotherDummy>();
            });

        }

        public class DoubleDummy : Identifiable { }
        public class DoubleDummyResourceDefinition1 : ResourceDefinition<DoubleDummy>
        {
            public DoubleDummyResourceDefinition1() : base(new ResourceGraphBuilder().AddResource<DoubleDummy>().Build()) { }

            public override IEnumerable<DoubleDummy> BeforeDelete(IEntityHashSet<DoubleDummy> affected, ResourcePipeline pipeline) { return affected; }
        }
        public class DoubleDummyResourceDefinition2 : ResourceDefinition<DoubleDummy>
        {
            public DoubleDummyResourceDefinition2() : base(new ResourceGraphBuilder().AddResource<DoubleDummy>().Build()) { }

            public override void AfterDelete(HashSet<DoubleDummy> entities, ResourcePipeline pipeline, bool succeeded) { }
        }
        [Fact]
        public void Multiple_Implementations_Of_ResourceDefinitions()
        {
            //  assert
            Assert.Throws<JsonApiSetupException>(() =>
            {
                // arrange & act
                var hookConfig = new HooksDiscovery<DoubleDummy>();
            });
        }
    }
}
