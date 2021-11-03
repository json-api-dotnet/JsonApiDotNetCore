using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class CompanyHealthInsurance : HealthInsurance
    {
        [Attr]
        public string CompanyCode { get; set; } = null!;
    }
}
