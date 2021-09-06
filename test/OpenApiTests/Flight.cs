using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Flight : Identifiable<int>
    {
        [Attr]
        public string Destination { get; set; }

        [Attr]
        public DateTimeOffset PlannedDeparture { get; set; }
    }
}
