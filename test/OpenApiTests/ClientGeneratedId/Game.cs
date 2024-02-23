using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ClientGeneratedId;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.ClientGeneratedId", ClientIdGeneration = ClientIdGenerationMode.Allowed)]
public sealed class Game : Identifiable<long>
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public decimal Price { get; set; }
}
