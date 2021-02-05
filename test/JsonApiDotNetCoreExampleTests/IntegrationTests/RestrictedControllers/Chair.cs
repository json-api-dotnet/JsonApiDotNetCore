using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RestrictedControllers
{
    public sealed class Chair : Identifiable
    {
        [Attr]
        public int LegCount { get; set; }
    }
}
