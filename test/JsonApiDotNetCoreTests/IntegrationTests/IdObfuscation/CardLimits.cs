using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdObfuscation;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class CardLimits
{
    [Attr]
    public int DailyLimit { get; set; }

    [Attr]
    public string? Currency { get; set; }
}