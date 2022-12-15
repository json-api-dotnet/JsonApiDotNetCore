using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations")]
public sealed class Lyric : Identifiable<long>
{
    [Attr]
    public string? Format { get; set; }

    [Attr]
    public string Text { get; set; } = null!;

    [Attr(Capabilities = AttrCapabilities.None)]
    public DateTimeOffset CreatedAt { get; set; }

    [HasOne(Capabilities = HasOneCapabilities.All & ~HasOneCapabilities.AllowSet)]
    public TextLanguage? Language { get; set; }

    [HasOne]
    public MusicTrack? Track { get; set; }
}
