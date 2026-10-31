using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public struct AppointmentTiming
{
    [Attr]
    public int BufferMinutes { get; set; }

    [Attr]
    public string? Note { get; set; }
}
