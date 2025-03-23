using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ResourceInheritance.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.ResourceInheritance")]
public abstract class Room : Identifiable<long>
{
    [Attr]
    [Required]
    public int? SurfaceInSquareMeters { get; set; }

    [HasOne]
    public Residence Residence { get; set; } = null!;
}
