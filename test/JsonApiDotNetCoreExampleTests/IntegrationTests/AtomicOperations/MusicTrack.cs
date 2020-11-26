using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations
{
    public sealed class MusicTrack : Identifiable<Guid>
    {
        [Attr]
        [Required]
        public string Title { get; set; }

        [Attr]
        [Range(1, 24 * 60)]
        public decimal LengthInSeconds { get; set; }

        [Attr]
        public string Genre { get; set; }

        [Attr]
        public DateTimeOffset ReleasedAt { get; set; }

        [HasOne]
        public RecordCompany OwnedBy { get; set; }

        public IList<Performer> Performers { get; set; }
    }
}
