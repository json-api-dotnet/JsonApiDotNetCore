using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class OneToOneDependent : IdentifiableWithAttribute
    {
        [HasOne]
        public OneToOnePrincipal Principal { get; set; }

        public int? PrincipalId { get; set; }
    }
}
