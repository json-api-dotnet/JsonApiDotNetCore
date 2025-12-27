using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Serialization;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Serialization")]
public sealed class MeetingAttendee : Identifiable<Guid>
{
    [Attr]
    public required string DisplayName { get; set; }

    [Attr]
    public required Address HomeAddress { get; set; }

    [HasOne]
    public Meeting? Meeting { get; set; }
}
