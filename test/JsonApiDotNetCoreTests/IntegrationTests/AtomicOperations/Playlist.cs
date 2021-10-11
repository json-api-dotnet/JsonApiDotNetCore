#nullable disable

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Playlist : Identifiable<long>
    {
        [Attr]
        [Required]
        public string Name { get; set; }

        [NotMapped]
        [Attr]
        public bool IsArchived => false;

        [HasMany]
        public IList<MusicTrack> Tracks { get; set; }
    }
}
