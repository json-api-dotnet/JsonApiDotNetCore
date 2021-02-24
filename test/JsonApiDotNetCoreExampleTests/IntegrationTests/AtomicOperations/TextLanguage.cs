using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class TextLanguage : Identifiable<Guid>
    {
        [Attr]
        public string IsoCode { get; set; }

        [NotMapped]
        [Attr(Capabilities = AttrCapabilities.None)]
        public Guid ConcurrencyToken
        {
            get => Guid.NewGuid();
            set => _ = value;
        }

        [HasMany]
        public ICollection<Lyric> Lyrics { get; set; }
    }
}
