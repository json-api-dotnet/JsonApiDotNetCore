using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations")]
public sealed class Playlist : Identifiable<long>
{
    [Attr]
    public string Name { get; set; } = null!;

    [NotMapped]
    [Attr]
    public bool IsArchived => false;

    [HasMany]
    public IList<MusicTrack> Tracks { get; set; } = new List<MusicTrack>();
}
