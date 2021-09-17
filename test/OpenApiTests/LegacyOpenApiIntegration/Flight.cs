using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.LegacyOpenApiIntegration
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Flight : Identifiable<string>
    {
        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowChange)]
        [Required]
        [MaxLength(40)]
        public string FinalDestination { get; set; }

        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowChange)]
        [MaxLength(2000)]
        public string StopOverDestination { get; set; }

        [Attr(PublicName = "operated-by", Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowChange)]
        public Airline Airline { get; set; }

        [Attr]
        public DateTime? DepartsAt { get; set; }

        [Attr]
        public DateTime? ArrivesAt { get; set; }

        [HasMany]
        public ISet<FlightAttendant> CabinCrewMembers { get; set; }

        [HasOne]
        public FlightAttendant Purser { get; set; }

        [Attr]
        [NotMapped]
        public ICollection<string> ServicesOnBoard { get; set; }

        [HasMany]
        public ICollection<Passenger> Passengers { get; set; }
    }
}
