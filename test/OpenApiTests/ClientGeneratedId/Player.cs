using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ClientGeneratedId;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.ClientGeneratedId", ClientIdGeneration = ClientIdGenerationMode.Required)]
public sealed class Player : Identifiable<long>
{
    [Attr]
    public string Name { get; set; } = null!;

    [HasMany]
    public List<Game> Games { get; set; } = [];
}
