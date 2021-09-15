using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.LegacyOpenApiIntegration
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Airplane : Identifiable<string>
    {
        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange)]
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange)]
        public DateTime ManufacturedAt { get; set; }

        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange)]
        public int? AirtimeInHours { get; set; }

        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowChange)]
        public DateTime? LastServicedAt { get; set; }

        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowChange)]
        public bool IsInMaintenance { get; set; }

        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowChange)]
        [MaxLength(2000)]
        public string SerialNumber { get; set; }

        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowCreate)]
        public int SeatingCapacity { get; set; }

        [Attr(PublicName = "airplane-type", Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowCreate)]
        public AircraftType AircraftType { get; set; }

        [HasMany]
        public ISet<Flight> Flights { get; set; }
    }
}
