using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ContentNegotiation
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Policy : Identifiable<int>
    {
        [Attr]
        public string Name { get; set; } = null!;
    }
}
