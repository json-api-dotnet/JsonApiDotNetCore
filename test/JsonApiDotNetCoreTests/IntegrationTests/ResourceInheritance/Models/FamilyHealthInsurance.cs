using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class FamilyHealthInsurance : HealthInsurance
    {
        [Attr]
        public int PermittedFamilySize { get; set; }
    }
}
