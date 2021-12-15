using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.NamingConventions;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class StaffMember : Identifiable<int>
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public int Age { get; set; }
}
