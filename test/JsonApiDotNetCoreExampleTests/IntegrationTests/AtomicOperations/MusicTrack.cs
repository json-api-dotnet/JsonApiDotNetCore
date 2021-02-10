using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations
{
    public sealed class MusicTrack : Identifiable<Guid>
    {
        [RegularExpression(@"(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$")]
        public override Guid Id { get; set; }

        [Attr]
        [Required]
        public string Title { get; set; }

        [Attr]
        [Range(1, 24 * 60)]
        public decimal? LengthInSeconds { get; set; }

        [Attr]
        public string Genre { get; set; }

        [Attr]
        public DateTimeOffset ReleasedAt { get; set; }

        [HasOne]
        public Lyric Lyric { get; set;}
        
        [HasOne]
        public RecordCompany OwnedBy { get; set; }

        [HasMany]
        public IList<Performer> Performers { get; set; }
    }
}
