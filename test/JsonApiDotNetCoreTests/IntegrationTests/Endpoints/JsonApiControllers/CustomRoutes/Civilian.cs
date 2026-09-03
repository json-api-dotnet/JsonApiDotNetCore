using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.CustomRoutes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.CustomRoutes")]
public sealed class Civilian : Identifiable<long>
{
    [Attr]
    public string Name { get; set; } = null!;

    [HasOne]
    public Town? LivesIn { get; set; }
}
