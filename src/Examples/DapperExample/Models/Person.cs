using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace DapperExample.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource]
public sealed class Person : Identifiable<long>
{
    [Attr]
    public string? FirstName { get; set; }

    [Attr]
    public required string LastName { get; set; }

    // Mistakenly includes AllowFilter, so we can test for the error produced.
    [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowFilter)]
    [NotMapped]
    public string DisplayName => FirstName != null ? $"{FirstName} {LastName}" : LastName;

    [HasOne]
    public LoginAccount? Account { get; set; }

    [HasMany]
    public ISet<TodoItem> OwnedTodoItems { get; set; } = new HashSet<TodoItem>();

    [HasMany]
    public ISet<TodoItem> AssignedTodoItems { get; set; } = new HashSet<TodoItem>();
}
