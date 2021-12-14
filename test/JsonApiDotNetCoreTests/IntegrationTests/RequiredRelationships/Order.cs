using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships")]
public sealed class Order : Identifiable<int>
{
    [Attr]
    public decimal Amount { get; set; }

    [HasOne]
    public Customer Customer { get; set; } = null!;

    [HasOne]
    public Shipment Shipment { get; set; } = null!;
}
