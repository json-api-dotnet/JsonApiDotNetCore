using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace DapperExample.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource]
public sealed class Tag : Identifiable<long>
{
    [Attr]
    [MinLength(1)]
    public required string Name { get; set; }

    [HasOne]
    public RgbColor? Color { get; set; }

    [HasOne]
    public TodoItem? TodoItem { get; set; }
}
