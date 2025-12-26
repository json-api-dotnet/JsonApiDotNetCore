using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.LegacyOpenApi;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.LegacyOpenApi")]
public sealed class Airplane : Identifiable<string>
{
    [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange)]
    [MaxLength(255)]
    public required string Name { get; set; }

    [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange)]
    [MaxLength(16)]
    public string? SerialNumber { get; set; }

    [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange)]
    public int? AirtimeInHours { get; set; }

    [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange)]
    public DateTime? LastServicedAt { get; set; }

    /// <summary>
    /// Gets the day on which this airplane was manufactured.
    /// </summary>
    [Attr]
    public DateTime ManufacturedAt { get; set; }

    [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowChange)]
    public bool IsInMaintenance { get; set; }

    [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowChange)]
    [MaxLength(85)]
    public string? ManufacturedInCity { get; set; }

    [Attr(Capabilities = AttrCapabilities.AllowView)]
    public AircraftKind Kind { get; set; }

    [HasMany]
    public ISet<Flight> Flights { get; set; } = new HashSet<Flight>();
}
