using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations
{
    public sealed class Performer : Identifiable
    {
        [Attr]
        public string ArtistName { get; set; }

        [Attr]
        public DateTimeOffset BornAt { get; set; }
    }
}
