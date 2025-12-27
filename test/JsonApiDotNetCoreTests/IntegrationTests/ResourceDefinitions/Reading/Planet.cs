using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading")]
public sealed class Planet : Identifiable<long>
{
    [Attr]
    public required string PublicName { get; set; }

    [Attr]
    public string? PrivateName { get; set; }

    [Attr]
    public bool HasRingSystem { get; set; }

    [Attr]
    public decimal SolarMass { get; set; }

    [HasMany]
    public ISet<Moon> Moons { get; set; } = new HashSet<Moon>();

    [HasOne]
    public Star? BelongsTo { get; set; }
}
