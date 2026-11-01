using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class AppointmentAddress
{
    [Attr]
    public string? Street { get; set; }

    [Attr]
    public string? City { get; set; }

    [Attr]
    public string? CountryCode { get; set; }
}
