using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Serialization
{
    public sealed class MeetingAttendee : Identifiable<Guid>
    {
        [Attr]
        public string DisplayName { get; set; }

        [HasOne]
        public Meeting Meeting { get; set; }
    }
}
