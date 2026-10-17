using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound")]
public sealed class Parking : Listing
{
    [Attr(IsCompound = true)]
    public ParkingDimensions? Dimensions { get; set; }

    [Attr(IsCompound = true)]
    public ParkingInsurance? Insurance { get; set; }
}
