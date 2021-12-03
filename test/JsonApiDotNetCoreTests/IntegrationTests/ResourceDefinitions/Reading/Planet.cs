using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading")]
    public sealed class Planet : Identifiable<int>
    {
        [Attr]
        public string PublicName { get; set; } = null!;

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
}
