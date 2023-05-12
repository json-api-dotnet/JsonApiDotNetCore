using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace NoEntityFrameworkExample.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(GenerateControllerEndpoints = JsonApiEndpoints.Query)]
public sealed class TodoItem : Identifiable<long>
{
    [Attr]
    public string Description { get; set; } = null!;

    [Attr]
    [Required]
    public TodoItemPriority? Priority { get; set; }

    [Attr]
    public long? DurationInHours { get; set; }

    [HasOne]
    public Person Owner { get; set; } = null!;

    [HasOne]
    public Person? Assignee { get; set; }

    [HasMany]
    public ISet<Tag> Tags { get; set; } = new HashSet<Tag>();
}
