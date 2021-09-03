using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Moon : Identifiable
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public decimal SolarRadius { get; set; }

        [HasOne]
        public Planet OrbitsAround { get; set; }
    }
}
