using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace BackgroundWorkerService.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource]
public sealed class Tag : Identifiable<int>
{
    [Attr]
    [MinLength(1)]
    public string Name { get; set; } = null!;

    [HasMany]
    public ISet<TodoItem> TodoItems { get; set; } = new HashSet<TodoItem>();
}
