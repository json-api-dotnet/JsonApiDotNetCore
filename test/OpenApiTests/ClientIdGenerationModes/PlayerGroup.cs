using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ClientIdGenerationModes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.ClientIdGenerationModes", ClientIdGeneration = ClientIdGenerationMode.Forbidden)]
public sealed class PlayerGroup : Identifiable<Guid>
{
    [Attr]
    public string Name { get; set; } = null!;

    [HasMany]
    public List<Player> Players { get; set; } = [];
}
