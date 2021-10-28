#nullable disable

using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class OneToManyDependent : IdentifiableWithAttribute
    {
        [HasOne]
        public OneToManyPrincipal Principal { get; set; }

        public int? PrincipalId { get; set; }
    }
}
