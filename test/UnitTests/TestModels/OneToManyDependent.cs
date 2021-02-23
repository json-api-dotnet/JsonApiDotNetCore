using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    public sealed class OneToManyDependent : IdentifiableWithAttribute
    {
        [HasOne]
        public OneToManyPrincipal Principal { get; set; }

        public int? PrincipalId { get; set; }
    }
}
