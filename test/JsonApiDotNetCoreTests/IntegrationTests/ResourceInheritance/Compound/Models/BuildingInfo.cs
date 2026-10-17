using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class BuildingInfo
{
    [Attr]
    public string? Name { get; set; }

    [Attr]
    [Range(1, 100)]
    public int Floor { get; set; }

    [Attr]
    public bool HasElevator { get; set; }
}
