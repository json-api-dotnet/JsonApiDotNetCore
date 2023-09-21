using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ZeroKeys;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ZeroKeys", ClientIdGeneration = ClientIdGenerationMode.Allowed)]
public sealed class Map : Identifiable<Guid?>
{
    [Attr]
    public string Name { get; set; } = null!;

    [HasOne]
    public Game? Game { get; set; }
}
