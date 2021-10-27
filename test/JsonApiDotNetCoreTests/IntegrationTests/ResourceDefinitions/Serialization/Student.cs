using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Serialization
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Student : Identifiable<int>
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public string SocialSecurityNumber { get; set; }

        [HasOne]
        public Scholarship Scholarship { get; set; }
    }
}
