using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ControllerActionResults
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Toothbrush : Identifiable
    {
        [Attr]
        public bool IsElectric { get; set; }
    }
}
