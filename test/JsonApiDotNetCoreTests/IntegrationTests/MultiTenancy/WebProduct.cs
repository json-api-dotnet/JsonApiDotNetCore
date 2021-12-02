using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy")]
    public sealed class WebProduct : Identifiable<int>
    {
        [Attr]
        public string Name { get; set; } = null!;

        [Attr]
        public decimal Price { get; set; }

        [HasOne]
        public WebShop Shop { get; set; } = null!;
    }
}
