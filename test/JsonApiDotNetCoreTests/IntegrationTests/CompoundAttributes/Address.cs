using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Owned]
public sealed class Address
{
    [Attr]
    public string Line1 { get; set; } = null!;

    [Attr]
    public string? Line2 { get; set; }

    [Attr]
    public string City { get; set; } = null!;

    [Attr]
    public string Country { get; set; } = null!;

    [Attr]
    public string PostalCode { get; set; } = null!;
}
