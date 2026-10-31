using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public struct AppointmentCoordinates
{
    [Attr]
    public double Latitude { get; set; }

    [Attr]
    public double Longitude { get; set; }

    [Attr(IsCompound = true)]
    public AppointmentAddress? NearestAddress { get; set; }
}
