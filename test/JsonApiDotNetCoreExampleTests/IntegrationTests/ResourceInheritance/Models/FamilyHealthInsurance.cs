using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Models
{
    public class FamilyHealthInsurance : HealthInsurance
    {
        [Attr]
        public int PermittedFamilySize  { get; set; }
    }
}
