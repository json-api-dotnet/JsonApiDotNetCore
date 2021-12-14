using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.EagerLoading;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[NoResource]
public sealed class Door
{
    public int Id { get; set; }
    public string Color { get; set; } = null!;
}
