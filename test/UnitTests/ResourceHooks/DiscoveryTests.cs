using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Hooks;
using System.Collections.Generic;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public class DiscoveryTests
    {
        [Fact]
        public void Hook_Discovery()
        {
            // arrange & act
            var hookConfig = new HooksDiscovery<Dummy>();
            // assert
            Assert.Contains(ResourceHook.BeforeDelete, hookConfig.ImplementedHooks);
            Assert.Contains(ResourceHook.AfterDelete, hookConfig.ImplementedHooks);

        }
        public class Dummy : Identifiable { }
        public class DummyResourceDefinition : ResourceDefinition<Dummy>
        {
            public override IEnumerable<Dummy> BeforeDelete(HashSet<Dummy> entities, ResourceAction pipeline) { return entities; }
            public override void AfterDelete(HashSet<Dummy> entities, ResourceAction pipeline, bool succeeded) {  }
        }
    }
}
