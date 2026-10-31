using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes.Constraints;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class AuthorContact
{
    [Attr]
    public string Email { get; set; } = null!;

    [Attr]
    public string City { get; set; } = null!;
}
