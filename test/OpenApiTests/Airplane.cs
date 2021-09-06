using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Airplane : Identifiable<int>
    {
        [Attr]
        public int SeatingCapacity { get; set; }

        [Attr]
        public DateTimeOffset ManufacturedAt { get; set; }

        [HasMany]
        public ISet<Flight> Flights { get; set; }
    }
}
