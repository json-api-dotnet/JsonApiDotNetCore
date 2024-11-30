using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ResourceInheritance.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.ResourceInheritance")]
public class Residence : Building
{
    [Attr]
    [Required]
    public int? NumberOfResidents { get; set; }

    [HasMany]
    public ISet<Room> Rooms { get; set; } = new HashSet<Room>();
}
