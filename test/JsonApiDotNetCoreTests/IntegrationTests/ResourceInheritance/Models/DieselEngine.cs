using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance")]
public sealed class DieselEngine : Engine
{
    [Attr]
    public override bool IsHydrocarbonBased { get; set; }

    [Attr]
    public string? SerialCode { get; set; }

    [Attr]
    public decimal Viscosity { get; set; }
}
