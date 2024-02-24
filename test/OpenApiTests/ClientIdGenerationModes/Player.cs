using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ClientIdGenerationModes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.ClientIdGenerationModes", ClientIdGeneration = ClientIdGenerationMode.Required)]
public sealed class Player : Identifiable<Guid>
{
    [Attr]
    public string UserName { get; set; } = null!;

    [HasMany]
    public List<Game> OwnedGames { get; set; } = [];

    [HasMany]
    public List<PlayerGroup> GroupMemberships { get; set; } = [];
}
