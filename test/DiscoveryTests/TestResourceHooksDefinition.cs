using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;

namespace DiscoveryTests
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class TestResourceHooksDefinition : ResourceHooksDefinition<TestResource>
    {
        public TestResourceHooksDefinition(IResourceGraph resourceGraph)
            : base(resourceGraph)
        {
        }
    }
}
