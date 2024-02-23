using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ClientGeneratedId;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.ClientGeneratedId", ClientIdGeneration = ClientIdGenerationMode.Forbidden)]
public sealed class Group : Identifiable<Guid>
{
    [Attr]
    public string Name { get; set; } = null!;

    [HasMany]
    public List<Player> Players { get; set; } = [];
}
