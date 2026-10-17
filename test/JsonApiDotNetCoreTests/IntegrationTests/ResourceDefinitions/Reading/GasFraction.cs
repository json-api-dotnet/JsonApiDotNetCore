using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class GasFraction
{
    [Attr]
    public string Gas { get; set; } = null!;

    [Attr]
    public decimal Fraction { get; set; }
}
