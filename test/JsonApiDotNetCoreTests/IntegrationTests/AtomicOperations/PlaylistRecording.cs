using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class PlaylistRecording
{
    [Attr]
    public string? Studio { get; set; }

    [Attr]
    public DateOnly? RecordedAt { get; set; }

    [Attr]
    public string? Engineer { get; set; }
}
