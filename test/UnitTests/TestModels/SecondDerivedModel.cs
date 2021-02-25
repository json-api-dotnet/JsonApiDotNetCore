using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class SecondDerivedModel : BaseModel
    {
        [Attr]
        public bool SecondProperty { get; set; }
    }
}
