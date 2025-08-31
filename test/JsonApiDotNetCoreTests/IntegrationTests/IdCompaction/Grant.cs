using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdCompaction;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(GenerateControllerEndpoints = JsonApiEndpoints.None)]
public sealed class Grant : CompactIdentifiable
{
    [Attr]
    public string Name { get; set; } = null!;
}
