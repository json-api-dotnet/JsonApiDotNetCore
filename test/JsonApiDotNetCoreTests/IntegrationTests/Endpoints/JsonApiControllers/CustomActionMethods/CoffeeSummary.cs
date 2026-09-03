using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.CustomActionMethods;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.CustomActionMethods",
    GenerateControllerEndpoints = JsonApiEndpoints.None)]
public sealed class CoffeeSummary : Identifiable<long>
{
    [Attr]
    public int TotalCount { get; set; }

    [Attr]
    public int BlackCount { get; set; }

    [Attr]
    public int OnlySugarCount { get; set; }

    [Attr]
    public int OnlyMilkCount { get; set; }

    [Attr]
    public int SugarWithMilkCount { get; set; }
}
