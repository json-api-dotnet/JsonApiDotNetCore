using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace NoEntityFrameworkExample.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource]
public sealed class WorkItem : Identifiable<int>
{
    [Attr]
    public bool IsBlocked { get; set; }

    [Attr]
    public string Title { get; set; } = null!;

    [Attr]
    public long DurationInHours { get; set; }

    [Attr]
    public Guid ProjectId { get; set; } = Guid.NewGuid();
}
