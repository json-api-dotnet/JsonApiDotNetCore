using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ControllerActionResults;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ControllerActionResults")]
public sealed class Toothbrush : Identifiable<int>
{
    [Attr]
    public bool IsElectric { get; set; }
}
