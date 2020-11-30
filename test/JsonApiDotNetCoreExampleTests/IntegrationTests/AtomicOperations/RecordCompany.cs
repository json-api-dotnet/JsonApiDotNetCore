using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations
{
    public sealed class RecordCompany : Identifiable<short>
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public string CountryOfResidence { get; set; }

        [HasMany]
        public IList<MusicTrack> Tracks { get; set; }

        [HasOne]
        public RecordCompany Parent { get; set; }
    }
}
