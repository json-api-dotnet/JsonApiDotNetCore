using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ClientIdGenerationModes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.ClientIdGenerationModes", ClientIdGeneration = ClientIdGenerationMode.Required,
    GenerateControllerEndpoints = JsonApiEndpoints.Post)]
public sealed class Player : Identifiable<Guid>
{
    [Attr]
    public required string UserName { get; set; }

    [HasMany]
    public List<Game> OwnedGames { get; set; } = [];

    [HasMany]
    public List<PlayerGroup> MemberOf { get; set; } = [];
}
