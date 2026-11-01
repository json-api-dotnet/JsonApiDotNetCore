using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class StarClassification
{
    [Attr]
    public string? SpectralClass { get; set; }

    [Attr]
    public string? LuminosityClass { get; set; }
}
