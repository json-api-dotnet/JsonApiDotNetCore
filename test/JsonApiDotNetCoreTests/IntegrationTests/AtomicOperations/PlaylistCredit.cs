using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class PlaylistCredit
{
    [Attr]
    public string? Role { get; set; }

    [Attr]
    [Required]
    public string PersonName { get; set; } = null!;
}
