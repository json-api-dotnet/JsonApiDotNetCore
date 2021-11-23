using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.CustomRoutes
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Civilian : Identifiable<int>
    {
        [Attr]
        public string Name { get; set; } = null!;
    }
}
