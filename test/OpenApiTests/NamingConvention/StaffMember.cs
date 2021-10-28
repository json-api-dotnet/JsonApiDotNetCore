using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.NamingConvention
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class StaffMember : Identifiable
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public int Age { get; set; }
    }
}
