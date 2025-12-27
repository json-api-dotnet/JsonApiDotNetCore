using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.LegacyOpenApi;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.LegacyOpenApi")]
public sealed class Passenger : Identifiable<string>
{
    [Attr(PublicName = "document-number", Capabilities = AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange)]
    [MaxLength(9)]
    public required string PassportNumber { get; set; }

    [Attr]
    public string? FullName { get; set; }

    [Attr]
    public CabinArea CabinArea { get; set; }
}
