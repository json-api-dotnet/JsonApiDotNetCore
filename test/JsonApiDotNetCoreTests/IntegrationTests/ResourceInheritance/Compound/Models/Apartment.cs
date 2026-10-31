using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound")]
public sealed class Apartment : Residence
{
    [Attr(IsCompound = true)]
    public BuildingInfo? Building { get; set; }

    [Attr(IsCompound = true)]
    public HomeInsurance? Insurance { get; set; }
}
