#nullable disable

using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesDisabled.RelationshipsObject;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.SchemaProperties")]
public sealed class HenHouse : Identifiable<int>
{
    [HasOne]
    public Chicken OldestChicken { get; set; }

    [Required]
    [HasOne]
    public Chicken FirstChicken { get; set; }

    [HasMany]
    public ICollection<Chicken> AllChickens { get; set; } = new HashSet<Chicken>();

    [Required]
    [HasMany]
    public ICollection<Chicken> ChickensReadyForLaying { get; set; } = new HashSet<Chicken>();
}
