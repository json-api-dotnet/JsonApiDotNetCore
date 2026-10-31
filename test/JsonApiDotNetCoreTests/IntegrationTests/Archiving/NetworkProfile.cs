using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Archiving;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class NetworkProfile
{
    [Attr]
    public string? Country { get; set; }

    [Attr]
    public int FoundedYear { get; set; }
}
