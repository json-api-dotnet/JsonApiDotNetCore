using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class RecordCompany : Identifiable<short>
    {
        [Attr]
        public string Name { get; set; } = null!;

        [Attr]
        public string? CountryOfResidence { get; set; }

        [HasMany]
        public IList<MusicTrack> Tracks { get; set; } = new List<MusicTrack>();

        [HasOne]
        public RecordCompany? Parent { get; set; }
    }
}
