using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ResourceInheritance;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.ResourceInheritance", GenerateControllerEndpoints = JsonApiEndpoints.None)]
public sealed class Toilet : Room
{
    [Attr]
    [Required]
    public bool? HasSink { get; set; }
}
