using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ClientIdGenerationModes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class GameSettings
{
    [Attr]
    public int MaxPlayers { get; set; }

    [Attr]
    public string? Difficulty { get; set; }
}
