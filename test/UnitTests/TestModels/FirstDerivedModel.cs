using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    public class FirstDerivedModel : BaseModel
    {
        [Attr]
        public bool FirstProperty { get; set; }
    }
}
