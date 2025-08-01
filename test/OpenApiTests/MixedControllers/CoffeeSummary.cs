using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.MixedControllers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.MixedControllers", GenerateControllerEndpoints = JsonApiEndpoints.None)]
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
