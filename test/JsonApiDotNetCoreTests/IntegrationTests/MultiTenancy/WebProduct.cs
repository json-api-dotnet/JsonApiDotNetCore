#nullable disable

using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class WebProduct : Identifiable<int>
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public decimal Price { get; set; }

        [HasOne]
        public WebShop Shop { get; set; }
    }
}
