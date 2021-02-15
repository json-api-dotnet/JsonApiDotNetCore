using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    public class OneToManyRequiredDependent : IdentifiableWithAttribute
    {
        [HasOne] public OneToManyPrincipal Principal { get; set; }
        public int PrincipalId { get; set; }
    }
}