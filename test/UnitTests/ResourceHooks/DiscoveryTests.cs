using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using System.Collections.Generic;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public class DiscoveryTests
    {
        public DiscoveryTests()
        {
            // Build() exposes the static ResourceGraphBuilder.Instance member, which 
            // is consumed by ResourceDefinition class.
            new ResourceGraphBuilder()
                .AddResource<TodoItem>()
                .AddResource<Person>()
                .Build();
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
        public class Dummy : Identifiable { }
        public class DummyResourceDefinition : ResourceDefinition<Dummy>
        {
            public override IEnumerable<Dummy> BeforeDelete(IEnumerable<Dummy> entities, HookExecutionContext<Dummy> context) { return entities; }
            public override IEnumerable<Dummy> AfterDelete(IEnumerable<Dummy> entities, HookExecutionContext<Dummy> context, bool succeeded) { return entities; }
        }
    }
}
