using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ClientIdGenerationModes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.ClientIdGenerationModes", ClientIdGeneration = ClientIdGenerationMode.Allowed,
    GenerateControllerEndpoints = JsonApiEndpoints.Post)]
public sealed class Game : Identifiable<Guid>
{
    [Attr]
    public string Title { get; set; } = null!;

    [Attr]
    public decimal PurchasePrice { get; set; }
}
