using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    public sealed class FirstDerivedModel : BaseModel
    {
        [Attr] public bool FirstProperty { get; set; }
    }
}
