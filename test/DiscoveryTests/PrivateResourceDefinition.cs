using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;

namespace DiscoveryTests
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class PrivateResourceDefinition : JsonApiResourceDefinition<PrivateResource, int>
    {
        public PrivateResourceDefinition(IResourceGraph resourceGraph)
            : base(resourceGraph)
        {
        }
    }
}
