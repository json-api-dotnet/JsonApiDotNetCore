using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers")]
    public sealed class Pillow : Identifiable<int>
    {
        [Attr]
        public string Color { get; set; } = null!;
    }
}
