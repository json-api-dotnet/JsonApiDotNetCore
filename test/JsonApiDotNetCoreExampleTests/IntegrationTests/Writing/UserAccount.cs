using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Writing
{
    // TODO: Why not just "User"? That would seem more intuitive to me.
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
