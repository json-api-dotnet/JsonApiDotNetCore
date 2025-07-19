using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.Links;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.Links")]
public sealed class Excursion : Identifiable<long>
{
    [Attr]
    [Required]
    public DateTime? OccursAt { get; set; }

    [Attr]
    public string Description { get; set; } = null!;
}
