using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings
{
    public sealed class Calendar : Identifiable
    {
        [Attr]
        public string TimeZone { get; set; }

        [Attr]
        public int DefaultAppointmentDurationInMinutes { get; set; }

        [HasMany]
        public ISet<Appointment> Appointments { get; set; }
    }
}
