using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ClientIdGenerationModes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ClientIdGenerationModes")]
public sealed class Tournament : Identifiable<long>
{
    [Attr]
    public string Title { get; set; } = null!;

    [HasMany]
    public List<Player> Players { get; set; } = [];
}
