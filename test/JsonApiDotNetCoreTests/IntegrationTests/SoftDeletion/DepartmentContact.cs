using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.SoftDeletion;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class DepartmentContact
{
    [Attr]
    public string Role { get; set; } = null!;

    [Attr]
    public string? Email { get; set; }
}
