using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Serialization;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class AttendeeProfile
{
    // The JSON property name is ignored for compound attributes, in contrast to non-compound complex attributes such as MeetingLocation.
    [Attr]
    [JsonPropertyName("nick")]
    public string? Nickname { get; set; }

    [Attr]
    public string? Biography { get; set; }
}
