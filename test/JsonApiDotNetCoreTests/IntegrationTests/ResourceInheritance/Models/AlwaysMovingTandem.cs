using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance", GenerateControllerEndpoints = JsonApiEndpoints.None)]
public sealed class AlwaysMovingTandem : Bike
{
    [NotMapped]
    [Attr]
    public Guid LocationToken
    {
        get => Guid.NewGuid();
        set => _ = value;
    }
}
