using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Flight : Identifiable
    {
        [Attr]
        public string Destination { get; set; }

        [Attr]
        public DateTimeOffset DepartsAt { get; set; }
    }
}
