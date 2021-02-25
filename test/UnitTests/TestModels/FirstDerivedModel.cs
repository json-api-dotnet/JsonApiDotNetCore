using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class FirstDerivedModel : BaseModel
    {
        [Attr]
        public bool FirstProperty { get; set; }
    }
}
