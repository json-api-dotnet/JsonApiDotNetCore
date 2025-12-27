using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

#pragma warning disable AV1115 // Member or local function contains the word 'and', which suggests doing multiple things

namespace JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships")]
public sealed class Shipment : Identifiable<long>
{
    [Attr]
    public required string TrackAndTraceCode { get; set; }

    [Attr]
    public DateTimeOffset ShippedAt { get; set; }

    [HasOne]
    public required Order Order { get; set; }
}
