using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ResourceInheritance;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.ResourceInheritance", GenerateControllerEndpoints = JsonApiEndpoints.None)]
public abstract class Room : Identifiable<long>
{
    [Attr]
    [Required]
    public int? SurfaceInSquareMeters { get; set; }

    [HasOne]
    public Residence Residence { get; set; } = null!;
}
