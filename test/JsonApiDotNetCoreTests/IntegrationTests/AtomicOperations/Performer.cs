using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Performer : Identifiable<int>
    {
        [Attr]
        public string? ArtistName { get; set; }

        [Attr]
        public DateTimeOffset BornAt { get; set; }
    }
}
