using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public class IdentifiableWithAttribute : Identifiable
    {
        [Attr]
        public string AttributeMember { get; set; }
    }
}
