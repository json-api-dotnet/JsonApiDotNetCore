using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.Links;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.Links")]
public sealed class Transport : Identifiable<long>
{
    [Attr]
    [Required]
    public TransportType? Type { get; set; }

    [Attr]
    [Required]
    public int? DurationInMinutes { get; set; }
}
