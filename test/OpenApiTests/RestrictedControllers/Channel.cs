using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.RestrictedControllers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public abstract class Channel : Identifiable<long>
{
    [Attr]
    public string? Name { get; set; }

    [HasOne]
    public DataStream VideoStream { get; set; } = null!;

    [HasMany]
    public ISet<DataStream> AudioStreams { get; set; } = new HashSet<DataStream>();
}
