using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance")]
public abstract class Engine : Identifiable<long>
{
    [Attr]
    public abstract bool IsHydrocarbonBased { get; set; }

    [Attr]
    public decimal Capacity { get; set; }
}
