using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace DapperExample.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource]
public sealed class TodoItem : Identifiable<long>
{
    [Attr]
    public required string Description { get; set; }

    [Attr]
    [Required]
    public TodoItemPriority? Priority { get; set; }

    [Attr]
    public long? DurationInHours { get; set; }

    [Attr(Capabilities = AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort | AttrCapabilities.AllowView)]
    public DateTimeOffset CreatedAt { get; set; }

    [Attr(PublicName = "modifiedAt", Capabilities = AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort | AttrCapabilities.AllowView)]
    public DateTimeOffset? LastModifiedAt { get; set; }

    [HasOne]
    public required Person Owner { get; set; }

    [HasOne]
    public Person? Assignee { get; set; }

    [HasMany]
    public ISet<Tag> Tags { get; set; } = new HashSet<Tag>();
}
