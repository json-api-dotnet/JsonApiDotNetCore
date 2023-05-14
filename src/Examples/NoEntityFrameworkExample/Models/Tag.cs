using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace NoEntityFrameworkExample.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(GenerateControllerEndpoints = JsonApiEndpoints.Query)]
public sealed class Tag : Identifiable<long>
{
    [Attr]
    [MinLength(1)]
    public string Name { get; set; } = null!;

    [HasMany]
    public ISet<TodoItem> TodoItems { get; set; } = new HashSet<TodoItem>();
}
