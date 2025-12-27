using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations")]
public sealed class RecordCompany : Identifiable<short>
{
    [Attr]
    public required string Name { get; set; }

    [Attr]
    public string? CountryOfResidence { get; set; }

    [HasMany]
    public IList<MusicTrack> Tracks { get; set; } = new List<MusicTrack>();

    [HasOne]
    public RecordCompany? Parent { get; set; }
}
