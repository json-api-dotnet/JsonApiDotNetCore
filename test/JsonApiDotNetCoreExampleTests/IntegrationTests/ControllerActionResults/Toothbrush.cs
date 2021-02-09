using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ControllerActionResults
{
    public sealed class Toothbrush : Identifiable
    {
        [Attr]
        public bool IsElectric { get; set; }
    }
}
