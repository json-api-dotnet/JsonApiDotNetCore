using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class OneToOnePrincipal : IdentifiableWithAttribute
    {
        [HasOne]
        public OneToOneDependent Dependent { get; set; }
    }
}
