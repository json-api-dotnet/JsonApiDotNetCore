using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ZeroKeys;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ZeroKeys", ClientIdGeneration = ClientIdGenerationMode.Allowed)]
public sealed class Player : Identifiable<string?>
{
    [Attr]
    public string EmailAddress { get; set; } = null!;

    [HasOne]
    public Game? ActiveGame { get; set; }

    [HasMany]
    public ICollection<Game> RecentlyPlayed { get; set; } = new List<Game>();
}
