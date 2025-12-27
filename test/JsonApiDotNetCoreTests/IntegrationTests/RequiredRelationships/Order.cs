using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships")]
public sealed class Order : Identifiable<long>
{
    [Attr]
    public decimal Amount { get; set; }

    [HasOne]
    public required Customer Customer { get; set; }

    [HasOne]
    public required Shipment Shipment { get; set; }
}
