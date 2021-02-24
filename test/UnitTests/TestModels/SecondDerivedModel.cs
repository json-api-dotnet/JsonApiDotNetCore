using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    public sealed class SecondDerivedModel : BaseModel
    {
        [Attr] public bool SecondProperty { get; set; }
    }
}
