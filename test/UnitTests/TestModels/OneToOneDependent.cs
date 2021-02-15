using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    public sealed class OneToOneDependent : IdentifiableWithAttribute
    {
        [HasOne] public OneToOnePrincipal Principal { get; set; }
        public int? PrincipalId { get; set; }
    }
}