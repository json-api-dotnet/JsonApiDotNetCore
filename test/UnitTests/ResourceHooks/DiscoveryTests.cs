
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExample.Resources;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var hookConfig = new ImplementedResourceHooks<Dummy>();
            // assert
            Assert.Contains(ResourceHook.BeforeDelete, hookConfig.ImplementedHooks);
            Assert.Contains(ResourceHook.AfterDelete, hookConfig.ImplementedHooks);

        }
        public class Dummy : Identifiable
        {

        }
        public class DummyResourceDefinition : ResourceDefinition<Dummy>
        {
            public override void BeforeDelete(IEnumerable<Dummy> entities, ResourceAction actionSource)
            {
            }
            public override void AfterDelete(IEnumerable<Dummy> entities, bool succeeded, ResourceAction actionSource)
            {

            }
        }
    }


}
