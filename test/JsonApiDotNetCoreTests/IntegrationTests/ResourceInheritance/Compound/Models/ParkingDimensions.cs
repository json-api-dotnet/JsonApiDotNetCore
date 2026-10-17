using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ParkingDimensions
{
    [Attr]
    public decimal Width { get; set; }

    [Attr]
    public decimal Depth { get; set; }
}
