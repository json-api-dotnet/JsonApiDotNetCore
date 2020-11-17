using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ReadWrite
{
    public sealed class UserAccount : Identifiable<long>
    {
        [Attr]
        public string FirstName { get; set; }

        [Attr]
        public string LastName { get; set; }

        [HasMany]
        public ISet<WorkItem> AssignedItems { get; set; }
    }
}
