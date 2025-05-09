using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Owned]
public sealed class PhoneNumber
{
    [Attr]
    [Required]
    public PhoneNumberType? Type { get; set; }

    [Attr]
    public int? CountryCode { get; set; }

    [Attr]
    public string Number { get; set; } = null!;
}
