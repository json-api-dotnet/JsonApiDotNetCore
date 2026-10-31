using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class CalendarAudit
{
    [Attr]
    public string? CreatedBy { get; set; }
}
