using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesEnabled;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.SchemaProperties")]
public sealed class Cow : Identifiable<int>
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    [Required]
    public string NameOfCurrentFarm { get; set; } = null!;

    [Attr]
    public string? NameOfPreviousFarm { get; set; }

    [Attr]
    [Required]
    public string? Nickname { get; set; }

    [Attr]
    public int Age { get; set; }

    [Attr]
    [Required]
    public int Weight { get; set; }

    [Attr]
    public int? TimeAtCurrentFarmInDays { get; set; }

    [Attr]
    [Required]
    public bool? HasProducedMilk { get; set; }
}
