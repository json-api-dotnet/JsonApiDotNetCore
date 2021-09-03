using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceDefinitions.Serialization
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Student : Identifiable
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public string SocialSecurityNumber { get; set; }

        [HasOne]
        public Scholarship Scholarship { get; set; }
    }
}
