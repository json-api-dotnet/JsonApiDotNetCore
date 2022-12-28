#nullable disable

using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ResourceFieldsValidation.NullableReferenceTypesDisabled;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.ResourceFieldsValidation")]
public sealed class Chicken : Identifiable<int>
{
    [Attr]
    public string Name { get; set; }

    [Attr]
    [Required]
    public string NameOfCurrentFarm { get; set; }

    [Attr]
    public int Age { get; set; }

    [Attr]
    [Required]
    public int Weight { get; set; }

    [Attr]
    public int? TimeAtCurrentFarmInDays { get; set; }

    [Attr]
    [Required]
    public bool? HasProducedEggs { get; set; }
}
