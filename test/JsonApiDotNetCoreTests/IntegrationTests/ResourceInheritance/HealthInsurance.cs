using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public abstract class HealthInsurance : Identifiable<int>
    {
        [Attr]
        public bool HasMonthlyFee { get; set; }
    }
}
