using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;

namespace GettingStarted.Models;

[Owned]
public sealed class Province
{
    [Attr]
    public required string Name { get; set; }
}
