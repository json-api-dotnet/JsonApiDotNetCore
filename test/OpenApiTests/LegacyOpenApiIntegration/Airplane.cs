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
        public string Name { get; set; } = null!;

        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange)]
        [MaxLength(16)]
        public string SerialNumber { get; set; } = null!;

        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange)]
        public int? AirtimeInHours { get; set; }

        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange)]
        public DateTime? LastServicedAt { get; set; }

        [Attr]
        public DateTime ManufacturedAt { get; set; }

        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowChange)]
        public bool IsInMaintenance { get; set; }

        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowChange)]
        [MaxLength(85)]
        public string ManufacturedInCity { get; set; } = null!;

        [Attr(Capabilities = AttrCapabilities.AllowView)]
        public AircraftKind Kind { get; set; }

        [HasMany]
        public ISet<Flight> Flights { get; set; } = null!;
    }
}
