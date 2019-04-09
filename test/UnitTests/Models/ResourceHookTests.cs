
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using System.Collections.Generic;
using Xunit;

namespace UnitTests.Models
{
    public class Dummy : Identifiable
    {

    }

    public class DummyResourceDefinition : ResourceDefinition<Dummy>
    {
        public override void BeforeDelete(Dummy entity, ResourceAction actionSource)
        {
        }
        public override void AfterDelete(Dummy entity, bool succeeded, ResourceAction actionSource)
        {

        }
    }

    public class ResourceHooks_Tests
    {

        public ResourceHooks_Tests()
        {

        }

        [Fact]
        public void Hook_Discovery()
        {
            var hookConfig = new ImplementedResourceHooks<Dummy>();
            Assert.Contains(ResourceHook.BeforeDelete, hookConfig.ImplementedHooks);
            Assert.Contains(ResourceHook.AfterDelete, hookConfig.ImplementedHooks);
            Assert.Equal(2, hookConfig.ImplementedHooks.Length);


        }
    }
}
