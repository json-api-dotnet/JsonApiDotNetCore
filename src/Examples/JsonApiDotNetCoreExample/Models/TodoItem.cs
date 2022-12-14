using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource]
public sealed class TodoItem : Identifiable<int>
{
    [Attr]
    public string Description { get; set; } = null!;

    [Attr]
    [Required]
    public TodoItemPriority? Priority { get; set; }

    [Attr(Capabilities = AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort | AttrCapabilities.AllowView)]
    public DateTimeOffset CreatedAt { get; set; }

    [Attr(PublicName = "modifiedAt", Capabilities = AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort | AttrCapabilities.AllowView)]
    public DateTimeOffset? LastModifiedAt { get; set; }

    [HasOne]
    public Person Owner { get; set; } = null!;

    [HasOne(Capabilities = HasOneCapabilities.AllowView | HasOneCapabilities.AllowSet)]
    public Person? Assignee { get; set; }

    [HasMany(Capabilities = HasManyCapabilities.AllowView | HasManyCapabilities.AllowFilter)]
    public ISet<Tag> Tags { get; set; } = new HashSet<Tag>();
}
