using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.ActionResults;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.ActionResults")]
public sealed class Toothbrush : Identifiable<long>
{
    [Attr]
    public bool IsElectric { get; set; }
}
