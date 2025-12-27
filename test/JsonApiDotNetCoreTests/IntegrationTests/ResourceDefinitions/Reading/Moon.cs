using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading")]
public sealed class Moon : Identifiable<long>
{
    [Attr]
    public required string Name { get; set; }

    [Attr]
    public decimal SolarRadius { get; set; }

    [HasOne]
    public required Planet OrbitsAround { get; set; }

    [HasOne]
    public Star? IsGivenLightBy { get; set; }
}
