using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations
{
    public sealed class TextLanguage : Identifiable<Guid>
    {
        [Attr]
        public string IsoCode { get; set; }

        [NotMapped]
        [Attr(Capabilities = AttrCapabilities.None)]
        public Guid ConcurrencyToken
        {
            get => Guid.NewGuid();
            set { }
        }

        [HasMany]
        public ICollection<Lyric> Lyrics { get; set; }
    }
}
