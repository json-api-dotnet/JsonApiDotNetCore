using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;

namespace GettingStarted.Models;

[Owned]
public sealed class Country
{
    [Attr]
    public required string Code { get; set; }

    [Attr]
    public string? DisplayName { get; set; }
}
