using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy")]
public sealed class WebProduct : Identifiable<long>
{
    [Attr]
    public required string Name { get; set; }

    [Attr]
    public decimal Price { get; set; }

    [HasOne]
    public required WebShop Shop { get; set; }
}
