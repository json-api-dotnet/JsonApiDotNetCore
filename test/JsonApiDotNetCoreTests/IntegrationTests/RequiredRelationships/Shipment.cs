using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships")]
public sealed class Shipment : Identifiable<long>
{
    [Attr]
    public string TrackAndTraceCode { get; set; } = null!;

    [Attr]
    public DateTimeOffset ShippedAt { get; set; }

    [HasOne]
    public Order Order { get; set; } = null!;
}
