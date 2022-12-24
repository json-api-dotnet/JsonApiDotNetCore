using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesEnabled.RelationshipsObject;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.SchemaProperties")]
public sealed class CowStable : Identifiable<int>
{
    [HasOne]
    public Cow OldestCow { get; set; } = null!;

    [Required]
    [HasOne]
    public Cow FirstCow { get; set; } = null!;

    [HasOne]
    public Cow? AlbinoCow { get; set; }

    [Required]
    [HasOne]
    public Cow? FavoriteCow { get; set; }

    [HasMany]
    public ICollection<Cow> CowsReadyForMilking { get; set; } = new HashSet<Cow>();

    [Required]
    [HasMany]
    public ICollection<Cow> AllCows { get; set; } = new HashSet<Cow>();
}
