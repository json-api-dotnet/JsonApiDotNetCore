using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.EagerLoading;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class BuildingDimensions
{
    [Attr]
    public decimal Width { get; set; }

    [Attr]
    public decimal Depth { get; set; }

    [Attr]
    public decimal Height { get; set; }
}
