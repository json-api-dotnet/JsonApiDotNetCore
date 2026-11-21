using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class AttendeeContact
{
    [Attr]
    public string? Phone { get; set; }

    [Attr]
    public string? Organization { get; set; }

    [Attr]
    public string? Title { get; set; }
}
