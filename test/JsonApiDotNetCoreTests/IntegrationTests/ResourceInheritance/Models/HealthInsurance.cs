#nullable disable

using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public abstract class HealthInsurance : Identifiable<int>
    {
        [Attr]
        public bool MonthlyFee { get; set; }
    }
}
