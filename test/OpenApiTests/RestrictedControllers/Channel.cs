using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.RestrictedControllers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public abstract class Channel : Identifiable<long>
{
    [Attr]
    public string? Name { get; set; }

    [Attr]
    public bool? IsCommercial { get; set; }

    [Attr]
    public bool? IsAdultOnly { get; set; }

    [HasOne]
    public DataStream VideoStream { get; set; } = null!;

    [HasOne]
    public DataStream? UltraHighDefinitionVideoStream { get; set; }

    [HasMany]
    public ISet<DataStream> AudioStreams { get; set; } = new HashSet<DataStream>();
}
