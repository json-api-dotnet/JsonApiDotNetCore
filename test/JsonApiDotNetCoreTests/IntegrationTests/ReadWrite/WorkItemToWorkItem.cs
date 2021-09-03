using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class WorkItemToWorkItem
    {
        public WorkItem FromItem { get; set; }
        public WorkItem ToItem { get; set; }
    }
}
