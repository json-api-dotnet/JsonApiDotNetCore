using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Star : Identifiable<int>
    {
        [Attr]
        public string Name { get; set; } = null!;

        [Attr]
        public StarKind Kind { get; set; }

        [Attr]
        public decimal SolarRadius { get; set; }

        [Attr]
        public decimal SolarMass { get; set; }

        [Attr]
        public bool IsVisibleFromEarth { get; set; }

        [HasMany]
        public ISet<Planet> Planets { get; set; } = new HashSet<Planet>();
    }
}
