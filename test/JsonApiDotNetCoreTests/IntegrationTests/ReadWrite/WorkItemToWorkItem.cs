using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[NoResource]
public sealed class WorkItemToWorkItem
{
    public required WorkItem FromItem { get; set; }
    public required WorkItem ToItem { get; set; }
}
