using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class AppointmentCost
{
    [Attr]
    public decimal Amount { get; set; }

    [Attr]
    public string? Currency { get; set; }
}
