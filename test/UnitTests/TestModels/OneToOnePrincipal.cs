using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    public sealed class OneToOnePrincipal : IdentifiableWithAttribute
    {
        [HasOne] public OneToOneDependent Dependent { get; set; }
    }
}