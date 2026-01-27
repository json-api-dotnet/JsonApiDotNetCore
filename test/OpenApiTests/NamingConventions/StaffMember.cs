using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.NamingConventions;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.NamingConventions")]
public sealed class StaffMember : Identifiable<long>
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public int Age { get; set; }
}
