using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class Room : Identifiable<int>
{
    [Attr]
    public int WindowCount { get; set; }
}
