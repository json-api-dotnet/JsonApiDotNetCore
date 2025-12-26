using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.NamingConventions;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.NamingConventions")]
public sealed class StaffMember : Identifiable<long>
{
    [Attr]
    public required string Name { get; set; }

    [Attr]
    public int Age { get; set; }
}
