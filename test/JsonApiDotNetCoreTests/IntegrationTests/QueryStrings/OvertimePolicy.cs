using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class OvertimePolicy
{
    [Attr]
    public List<DayOfWeek>? Days { get; set; }

    [Attr(IsCompound = true)]
    public List<OfficeHours>? Windows { get; set; }
}
