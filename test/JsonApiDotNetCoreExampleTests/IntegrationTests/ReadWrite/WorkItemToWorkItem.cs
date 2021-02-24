using JetBrains.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ReadWrite
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class WorkItemToWorkItem
    {
        public WorkItem FromItem { get; set; }
        public int FromItemId { get; set; }

        public WorkItem ToItem { get; set; }
        public int ToItemId { get; set; }
    }
}
