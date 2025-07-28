using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.MixedControllers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.MixedControllers", GenerateControllerEndpoints = JsonApiEndpoints.GetCollection | JsonApiEndpoints.Delete)]
public sealed class CupOfCoffee : Identifiable<long>
{
    [Attr]
    [Required]
    public bool? HasSugar { get; set; }

    [Attr]
    [Required]
    public bool? HasMilk { get; set; }
}
