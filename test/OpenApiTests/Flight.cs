using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

//
//
// [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange)]
// [Required, MaxLength(255)]
// public string Title { get; set; }
//
// [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange)]
// [MaxLength(2000)]
// public string Summary { get; set; }
//
// [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange)]
// public int? DurationInSeconds { get; set; }
//
// [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange)]
// public DateTime? PublishedAt { get; set; }
//
// [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowChange)]
// public bool IsDeleted { get; set; }
//
// [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowChange)]
// [MaxLength(2000)]
// public string ExternalId { get; set; }

namespace OpenApiTests
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Flight : Identifiable
    {
        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowChange)]
        [Required]
        [MaxLength(40)]
        public string Destination { get; set; }

        [Attr]
        public DateTime? DepartsAt { get; set; }

        [Attr]
        public DateTime? ArrivesAt { get; set; }

        [Attr(PublicName = "operated-by", Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowChange)]
        public Airline Airline { get; set; }

        [Attr]
        public ICollection<string> ServicesOnBoard { get; set; }

        [HasOne]
        public Airplane OperatingAirplane { get; set; }

        [HasMany(PublicName = "flight-attendants")]
        public ISet<FlightAttendant> CabinPersonnel { get; set; }

        [HasMany(PublicName = "reserve-flight-attendants")]
        public ICollection<FlightAttendant> BackupPersonnel { get; set; }
    }
}
