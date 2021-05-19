using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.MultiTenancy
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class WebProduct : Identifiable
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public decimal Price { get; set; }

        [HasOne]
        public WebShop Shop { get; set; }
    }
}
