using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading")]
    public sealed class Moon : Identifiable<int>
    {
        [Attr]
        public string Name { get; set; } = null!;

        [Attr]
        public decimal SolarRadius { get; set; }

        [HasOne]
        public Planet OrbitsAround { get; set; } = null!;
    }
}
