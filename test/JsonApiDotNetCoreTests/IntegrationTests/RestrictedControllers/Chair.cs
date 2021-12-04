using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(GenerateControllerEndpoints = NoRelationshipEndpoints, ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers")]
public sealed class Chair : Identifiable<int>
{
    private const JsonApiEndpoints NoRelationshipEndpoints = JsonApiEndpoints.GetCollection | JsonApiEndpoints.GetSingle | JsonApiEndpoints.Post |
        JsonApiEndpoints.Patch | JsonApiEndpoints.Delete;

    [Attr]
    public int LegCount { get; set; }

    [HasMany]
    public IList<Pillow> Pillows { get; set; } = new List<Pillow>();

    [HasOne]
    public Room? Room { get; set; }
}
