using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    public class SecondDerivedModel : BaseModel
    {
        [Attr] public bool SecondProperty { get; set; }
    }
}