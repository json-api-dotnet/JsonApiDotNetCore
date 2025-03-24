using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ResourceInheritance.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.ResourceInheritance")]
public sealed class Bathroom : Room
{
    [Attr]
    [Required]
    public bool? HasBath { get; set; }
}
