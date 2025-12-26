using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy")]
public sealed class WebShop : Identifiable<long>, IHasTenant
{
    [Attr]
    public required string Url { get; set; }

    public Guid TenantId { get; set; }

    [HasMany]
    public IList<WebProduct> Products { get; set; } = new List<WebProduct>();
}
