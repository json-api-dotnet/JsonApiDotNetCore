using JetBrains.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ReadWrite
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class WorkItemToWorkItem
    {
        public WorkItem FromItem { get; set; }
        public WorkItem ToItem { get; set; }
    }
}
