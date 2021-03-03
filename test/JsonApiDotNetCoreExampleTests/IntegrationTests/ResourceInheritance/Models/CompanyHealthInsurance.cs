using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class CompanyHealthInsurance : HealthInsurance
    {
        [Attr]
        public string CompanyCode { get; set; }
    }
}
