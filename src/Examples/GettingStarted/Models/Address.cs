using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;

namespace GettingStarted.Models;

[Owned]
public sealed class Address
{
    [Attr]
    public string? Street { get; set; }

    [Attr]
    public string? PostalCode { get; set; }

    [Attr(IsCompound = true)]
    public Country? Country { get; set; }

    public string? NotExposed { get; set; }
}
