using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ModelStateValidation
{
    public sealed class Enterprise : Identifiable
    {
        [Attr]
        [IsRequired]
        [RegularExpression(@"^[\w\s]+$")]
        public string CompanyName { get; set; }

        [Attr]
        [MinLength(5)]
        public string Industry { get; set; }

        [HasOne]
        public PostalAddress MailAddress { get; set; }

        [HasMany]
        public ICollection<EnterprisePartner> Partners { get; set; }

        [HasOne]
        public Enterprise Parent { get; set; }
    }
}
