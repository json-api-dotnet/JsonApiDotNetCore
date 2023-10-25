using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.DocComments;

/// <summary>
/// A space within a skyscraper, such as an office, hotel, residential space, or retail space.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.DocComments")]
public sealed class Space : Identifiable<long>
{
    /// <summary>
    /// The floor number on which this space resides.
    /// </summary>
    [Attr]
    public int FloorNumber { get; set; }

    /// <summary>
    /// The skyscraper this space exists in.
    /// </summary>
    [HasOne]
    public Skyscraper ExistsIn { get; set; } = null!;
}
