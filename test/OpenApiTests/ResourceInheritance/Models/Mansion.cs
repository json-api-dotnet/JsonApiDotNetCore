using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ResourceInheritance.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.ResourceInheritance")]
public sealed class Mansion : Residence
{
    [Attr]
    public string OwnerName { get; set; } = null!;

    [HasMany]
    public ISet<StaffMember> Staff { get; set; } = new HashSet<StaffMember>();
}
