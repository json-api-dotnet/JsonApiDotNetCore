using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Table : Identifiable
    {
        [Attr]
        public int LegCount { get; set; }
    }
}
