using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.LegacyOpenApiIntegration
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class FlightAttendant : Identifiable<string>
    {
        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowFilter)]
        public override string Id { get; set; } = null!;

        [Attr(Capabilities = AttrCapabilities.None)]
        public FlightAttendantExpertiseLevel ExpertiseLevel { get; set; }

        [Attr(Capabilities = AttrCapabilities.All)]
        [Required]
        [EmailAddress]
        public string? EmailAddress { get; set; }

        [Attr(Capabilities = AttrCapabilities.All)]
        [Range(18, 75)]
        public int Age { get; set; }

        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowCreate)]
        [Url]
        public string ProfileImageUrl { get; set; } = null!;

        [Attr]
        public long DistanceTraveledInKilometers { get; set; }

        [HasMany]
        public ISet<Flight> ScheduledForFlights { get; set; } = new HashSet<Flight>();

        [HasMany]
        public ISet<Flight> PurserOnFlights { get; set; } = new HashSet<Flight>();
    }
}
