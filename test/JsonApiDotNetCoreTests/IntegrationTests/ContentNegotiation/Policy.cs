using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ContentNegotiation
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Policy : Identifiable
    {
        [Attr]
        public string Name { get; set; }
    }
}
