using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class AppointmentLocation
{
    [Attr]
    public string? Name { get; set; }

    [Attr(IsCompound = true)]
    public AppointmentAddress? Address { get; set; }

    [Attr]
    public string? Room { get; set; }
}
