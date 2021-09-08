using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class FlightAttendant : Identifiable<long>
    {
        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowFilter)]
        public override long Id { get; set; }

        [Attr(Capabilities = AttrCapabilities.None)]
        public FlightAttendantExpertise ExpertiseLevel { get; set; }

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
        [Url]
        public string ProfileImageUrl { get; set; }

        [Attr(Capabilities = AttrCapabilities.All)]
        public ICollection<string> DestinationPreferences { get; set; }

        [HasMany]
        public ISet<Flight> Flights { get; set; }

        [HasOne]
        public Flight PurserOnFlight { get; set; }
    }
}
