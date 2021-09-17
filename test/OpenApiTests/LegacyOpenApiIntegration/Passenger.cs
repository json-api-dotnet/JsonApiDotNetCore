using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.LegacyOpenApiIntegration
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Passenger : Identifiable<string>
    {
        [Attr(PublicName = "document-number", Capabilities = AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange)]
        [Required]
        [MaxLength(9)]
        public string PassportNumber { get; set; }

        [Attr]
        public string FullName { get; set; }

        [Attr]
        public CabinArea CabinArea { get; set; }
    }
}
