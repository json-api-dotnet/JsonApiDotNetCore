using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class OfficeHours
{
    [Attr]
    public TimeOnly OpensAt { get; set; }

    [Attr]
    public TimeOnly ClosesAt { get; set; }
}
