using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations
{
    public sealed class Playlist : Identifiable<long>
    {
        [Attr]
        [Required]
        public string Name { get; set; }

        [NotMapped]
        [Attr]
        public bool IsArchived => false;

        [NotMapped]
        [HasManyThrough(nameof(PlaylistMusicTracks))]
        public IList<MusicTrack> Tracks { get; set; }

        public IList<PlaylistMusicTrack> PlaylistMusicTracks { get; set; }
    }
}
