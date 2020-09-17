using System.ComponentModel.DataAnnotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ModelStateValidation
{
    public sealed class EnterprisePartner : Identifiable
    {
        [Attr]
        [IsRequired]
        [MinLength(3)]
        public string Name { get; set; }

        [HasOne]
        public PostalAddress PrimaryMailAddress { get; set; }

        [Attr]
        [IsRequired]
        public EnterprisePartnerClassification Classification { get; set; }
    }
}
