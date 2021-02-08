using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RestrictedControllers
{
    public sealed class Bed : Identifiable
    {
        [Attr]
        public bool IsDouble { get; set; }
    }
}
