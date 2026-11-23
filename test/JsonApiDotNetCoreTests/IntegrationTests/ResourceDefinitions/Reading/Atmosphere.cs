using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class Atmosphere
{
    [Attr]
    public decimal Pressure { get; set; }

    [Attr(IsCompound = true)]
    public List<GasFraction> Composition { get; set; } = [];
}
