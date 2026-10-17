using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.NamingConventions;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class LaneReservation
{
    [Attr]
    public int LaneNumber { get; set; }

    [Attr(PublicName = "Booked-By")]
    public string? BookedBy { get; set; }
}
