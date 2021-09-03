using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class TextLanguage : Identifiable<Guid>
    {
        [Attr]
        public string IsoCode { get; set; }

        [Attr(Capabilities = AttrCapabilities.None)]
        public bool IsRightToLeft { get; set; }

        [HasMany]
        public ICollection<Lyric> Lyrics { get; set; }
    }
}
