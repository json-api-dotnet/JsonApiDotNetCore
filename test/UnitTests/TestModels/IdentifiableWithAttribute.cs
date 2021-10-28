using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public class IdentifiableWithAttribute : Identifiable<int>
    {
        [Attr]
        public string AttributeMember { get; set; }
    }
}
