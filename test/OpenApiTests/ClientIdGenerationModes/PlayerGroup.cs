using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ClientIdGenerationModes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.ClientIdGenerationModes", ClientIdGeneration = ClientIdGenerationMode.Forbidden,
    GenerateControllerEndpoints = JsonApiEndpoints.Post)]
public sealed class PlayerGroup : Identifiable<long>
{
    [Attr]
    public required string Name { get; set; }

    [HasMany]
    public List<Player> Players { get; set; } = [];
}
