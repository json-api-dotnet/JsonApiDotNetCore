using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class WorkTag : Identifiable<int>
{
    [Attr]
    public string Text { get; set; } = null!;

    [Attr]
    public bool IsBuiltIn { get; set; }

    [HasMany]
    public ISet<WorkItem> WorkItems { get; set; } = new HashSet<WorkItem>();
}
