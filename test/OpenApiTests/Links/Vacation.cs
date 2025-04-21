using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.Links;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.Links")]
public sealed class Vacation : Identifiable<long>
{
    [Attr]
    [Required]
    public DateTime? StartsAt { get; set; }

    [Attr]
    [Required]
    public DateTime? EndsAt { get; set; }

    [HasOne]
    public Accommodation Accommodation { get; set; } = null!;

    [HasOne]
    public Transport? Transport { get; set; }

    [HasMany]
    public ISet<Excursion> Excursions { get; set; } = new HashSet<Excursion>();
}
