using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RestrictedControllers
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Chair : Identifiable
    {
        [Attr]
        public int LegCount { get; set; }
    }
}
