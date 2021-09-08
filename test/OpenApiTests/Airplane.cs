using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Airplane : Identifiable<string>
    {
        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowCreate)]
        public int SeatingCapacity { get; set; }

        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowCreate)]
        public long SerialNumber { get; set; }

        [Attr(PublicName = "airplane-type", Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowCreate)]
        public AircraftType AircraftType { get; set; }

        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowCreate)]
        public DateTime ManufacturedAt { get; set; }

        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowChange)]
        public DateTime? LastServicedAt { get; set; }

        [HasMany]
        public ISet<Flight> Flights { get; set; }
    }
}
