using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Lyric : Identifiable<long>
    {
        [Attr]
        public string Format { get; set; }

        [Attr]
        public string Text { get; set; }

        [HasOne]
        public TextLanguage Language { get; set; }

        [Attr(Capabilities = AttrCapabilities.None)]
        public DateTimeOffset CreatedAt { get; set; }

        [HasOne]
        public MusicTrack Track { get; set; }
    }
}
