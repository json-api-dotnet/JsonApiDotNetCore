using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.Writing
{
    public sealed class WorkItem : Identifiable
    {
        [Attr]
        public string Description { get; set; }

        [Attr]
        public DateTime? DueAt { get; set; }

        [Attr(Capabilities = ~(AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange))]
        public Guid ConcurrencyToken { get; set; } = Guid.NewGuid();

        [HasOne]
        public UserAccount AssignedTo { get; set; }

        [HasMany]
        public ISet<UserAccount> Subscribers { get; set; }

        [HasOne]
        public WorkItemGroup Group { get; set; }
    }
}
