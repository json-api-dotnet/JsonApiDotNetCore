using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource]
public sealed class Person : Identifiable<long>
{
    [Attr]
    public string? FirstName { get; set; }

    [Attr]
    public string LastName { get; set; } = null!;

    [Attr(Capabilities = AttrCapabilities.AllowView)]
    [NotMapped]
    public string DisplayName => FirstName != null ? $"{FirstName} {LastName}" : LastName;

    [HasMany]
    public ISet<TodoItem> OwnedTodoItems { get; set; } = new HashSet<TodoItem>();

    [HasMany]
    public ISet<TodoItem> AssignedTodoItems { get; set; } = new HashSet<TodoItem>();
}
