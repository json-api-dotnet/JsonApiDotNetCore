using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.CustomRoutes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.CustomRoutes")]
public sealed class Civilian : Identifiable<int>
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    [Range(1900, 2050)]
    public int YearOfBirth { get; set; }
}
