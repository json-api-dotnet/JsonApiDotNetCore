using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace MultiDbContextExample.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource]
public sealed class ResourceB : Identifiable<long>
{
    [Attr]
    public string? NameB { get; set; }
}
