using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance")]
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
