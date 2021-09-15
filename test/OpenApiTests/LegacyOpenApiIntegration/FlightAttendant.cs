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
        public override string Id { get; set; }

        [Attr(Capabilities = AttrCapabilities.None)]
        public FlightAttendantExpertiseLevel ExpertiseLevel { get; set; }

        [Attr(PublicName = "document-number", Capabilities = AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange)]
        [Required]
        [MaxLength(9)]
        public string PassportNumber { get; set; }

        [Attr(Capabilities = AttrCapabilities.All)]
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Attr(Capabilities = AttrCapabilities.All)]
        [Required]
        [Range(18, 75)]
        public int Age { get; set; }

        [Attr(Capabilities = AttrCapabilities.All)]
        [Required]
        [Url]
        public string ProfileImageUrl { get; set; }

        [HasMany]
        public ISet<Flight> ScheduledForFlights { get; set; }

        [HasMany]
        public ISet<Flight> StandbyForFlights { get; set; }
    }
}
