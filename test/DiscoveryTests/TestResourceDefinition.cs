using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;

namespace DiscoveryTests
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class TestResourceDefinition : JsonApiResourceDefinition<TestResource>
    {
        public TestResourceDefinition(IResourceGraph resourceGraph)
            : base(resourceGraph)
        {
        }
    }
}
