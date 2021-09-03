using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Performer : Identifiable
    {
        [Attr]
        public string ArtistName { get; set; }

        [Attr]
        public DateTimeOffset BornAt { get; set; }
    }
}
