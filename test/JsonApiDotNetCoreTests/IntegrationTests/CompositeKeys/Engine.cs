using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompositeKeys
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Engine : Identifiable
    {
        [Attr]
        public string SerialCode { get; set; }

        [HasOne]
        public Car Car { get; set; }
    }
}
