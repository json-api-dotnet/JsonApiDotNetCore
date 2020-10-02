using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Models
{
    public class CompanyHealthInsurance : HealthInsurance
    {
        [Attr]
        public string CompanyCode { get; set; }
    }
}
