using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Serialization
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Serialization")]
    public sealed class MeetingAttendee : Identifiable<Guid>
    {
        [Attr]
        public string DisplayName { get; set; } = null!;

        [HasOne]
        public Meeting? Meeting { get; set; }
    }
}
