using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource]
public sealed class Person : Identifiable<int>
{
    [Attr]
    public string? FirstName { get; set; }

    [Attr]
    public string LastName { get; set; } = null!;

    [HasMany]
    public ISet<TodoItem> AssignedTodoItems { get; set; } = new HashSet<TodoItem>();
}
