using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ShopContact
{
    [Attr]
    public string? Email { get; set; }

    [Attr]
    public string? Phone { get; set; }
}
