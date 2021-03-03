using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
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
