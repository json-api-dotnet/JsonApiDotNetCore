using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class FamilyHealthInsurance : HealthInsurance
{
    [Attr]
    public int PermittedFamilySize { get; set; }
}
