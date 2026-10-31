using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class Attendee
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public string? Email { get; set; }

    [Attr]
    public AttendeeResponse? Response { get; set; }
}
