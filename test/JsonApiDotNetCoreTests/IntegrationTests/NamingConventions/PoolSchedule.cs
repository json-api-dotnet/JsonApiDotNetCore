using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.NamingConventions;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class PoolSchedule
{
    [Attr]
    [Required]
    public TimeOnly? OpensAt { get; set; }

    [Attr]
    public TimeOnly ClosesAt { get; set; }

    [Attr]
    public bool IsHeated { get; set; }
}
