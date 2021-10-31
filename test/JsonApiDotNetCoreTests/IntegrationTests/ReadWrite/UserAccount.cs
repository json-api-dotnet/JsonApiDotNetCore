using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class UserAccount : Identifiable<long>
    {
        [Attr]
        public string FirstName { get; set; } = null!;

        [Attr]
        public string LastName { get; set; } = null!;

        [HasMany]
        public ISet<WorkItem> AssignedItems { get; set; } = new HashSet<WorkItem>();
    }
}
