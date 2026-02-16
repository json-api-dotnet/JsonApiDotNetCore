using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.Documentation;

/// <summary>
/// An elevator within a skyscraper.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.Documentation", ClientIdGeneration = ClientIdGenerationMode.Forbidden,
    GenerateControllerEndpoints = JsonApiEndpoints.None)]
public sealed class Elevator : Identifiable<long>
{
    /// <summary>
    /// The number of floors this elevator provides access to.
    /// </summary>
    [Attr]
    public int FloorCount { get; set; }

    /// <summary>
    /// The skyscraper this elevator exists in.
    /// </summary>
    [HasOne]
    public Skyscraper ExistsIn { get; set; } = null!;
}
