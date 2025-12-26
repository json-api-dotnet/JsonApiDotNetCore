using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations")]
public sealed class MusicTrack : Identifiable<Guid>
{
    [RegularExpression("(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$")]
    public override Guid Id { get; set; }

    [Attr]
    public required string Title { get; set; }

    [Attr]
    [Range(1, 24 * 60)]
    public decimal? LengthInSeconds { get; set; }

    [Attr]
    public string? Genre { get; set; }

    [Attr]
    [DateMustBeInThePast]
    public DateTimeOffset ReleasedAt { get; set; }

    [HasOne]
    public Lyric? Lyric { get; set; }

    [HasOne]
    public RecordCompany? OwnedBy { get; set; }

    [HasMany]
    public IList<Performer> Performers { get; set; } = new List<Performer>();

    [HasMany(Capabilities = HasManyCapabilities.All & ~(HasManyCapabilities.AllowSet | HasManyCapabilities.AllowAdd | HasManyCapabilities.AllowRemove))]
    public IList<Playlist> OccursIn { get; set; } = new List<Playlist>();
}
