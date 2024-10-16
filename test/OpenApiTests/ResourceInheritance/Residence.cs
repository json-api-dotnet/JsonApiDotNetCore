using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ResourceInheritance;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.ResourceInheritance", GenerateControllerEndpoints = JsonApiEndpoints.None)]
public class Residence : Building
{
    [Attr]
    [Required]
    public int? NumberOfResidents { get; set; }

    [HasMany]
    public ISet<Room> Rooms { get; set; } = new HashSet<Room>();
}
