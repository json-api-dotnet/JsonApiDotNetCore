using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompositeKeys
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.CompositeKeys")]
    public sealed class Engine : Identifiable<int>
    {
        [Attr]
        public string SerialCode { get; set; } = null!;

        [HasOne]
        public Car? Car { get; set; }
    }
}
