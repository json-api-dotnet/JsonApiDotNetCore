using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class Appointment : Identifiable<int>
{
    [Attr]
    public string Title { get; set; } = null!;

    [Attr]
    public string? Description { get; set; }

    [Attr]
    public DateTimeOffset StartTime { get; set; }

    [Attr]
    public DateTimeOffset EndTime { get; set; }
}
