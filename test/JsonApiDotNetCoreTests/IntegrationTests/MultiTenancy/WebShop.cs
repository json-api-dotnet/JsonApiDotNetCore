using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy")]
    public sealed class WebShop : Identifiable<int>, IHasTenant
    {
        [Attr]
        public string Url { get; set; } = null!;

        public Guid TenantId { get; set; }

        [HasMany]
        public IList<WebProduct> Products { get; set; } = new List<WebProduct>();
    }
}
