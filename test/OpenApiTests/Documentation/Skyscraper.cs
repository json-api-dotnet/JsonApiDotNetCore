using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.Documentation;

/// <summary>
/// A tall, continuously habitable building having multiple floors.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.Documentation")]
public sealed class Skyscraper : Identifiable<long>
{
    /// <summary>
    /// The height of this building, in meters.
    /// </summary>
    [Attr]
    [Required]
    public int? HeightInMeters { get; set; }

    /// <summary>
    /// An optional elevator within this building, providing access to spaces.
    /// </summary>
    [HasOne]
    public Elevator? Elevator { get; set; }

    /// <summary>
    /// The spaces within this building.
    /// </summary>
    [HasMany]
    public ISet<Space> Spaces { get; set; } = new HashSet<Space>();
}
