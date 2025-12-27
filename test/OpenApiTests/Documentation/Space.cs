using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.Documentation;

/// <summary>
/// A space within a skyscraper, such as an office, hotel, residential space, or retail space.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.Documentation")]
public sealed class Space : Identifiable<long>
{
    /// <summary>
    /// The floor number on which this space resides.
    /// </summary>
    [Attr]
    public int FloorNumber { get; set; }

    /// <summary>
    /// The kind of this space.
    /// </summary>
    [Attr]
    public SpaceKind Kind { get; set; }

    /// <summary>
    /// The skyscraper this space exists in.
    /// </summary>
    [HasOne]
    public required Skyscraper ExistsIn { get; set; }
}
