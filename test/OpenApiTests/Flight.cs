using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Flight : Identifiable
    {
        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowCreate)]
        public string Destination { get; set; }

        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange)]
        public DateTimeOffset DepartsAt { get; set; }

        [HasOne]
        public Airplane ServicingAirplane { get; set; }

        [HasMany(PublicName = "flight-attendants")]
        public ISet<FlightAttendant> CabinPersonnel { get; set; }

        [HasOne]
        public FlightAttendant Purser { get; set; }
    }
}
