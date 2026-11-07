using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class GardenInfo
{
    [Attr]
    public int AreaInSqm { get; set; }

    [Attr]
    public List<string> Trees { get; set; } = [];
}
