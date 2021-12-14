using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[NoResource]
public sealed class WorkItemToWorkItem
{
    public WorkItem FromItem { get; set; } = null!;
    public WorkItem ToItem { get; set; } = null!;
}
