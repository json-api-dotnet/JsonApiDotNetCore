using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Writing
{
    public sealed class WorkItem : Identifiable
    {
        [Attr]
        public string Description { get; set; }

        [Attr]
        public DateTime? DueAt { get; set; }

        [Attr]
        public WorkItemPriority Priority { get; set; }

        [NotMapped]
        [Attr(Capabilities = ~(AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange))]
        public Guid ConcurrencyToken { get; set; } = Guid.NewGuid();

        [HasOne]
        public UserAccount AssignedTo { get; set; }

        [HasMany]
        public ISet<UserAccount> Subscribers { get; set; }

        [NotMapped]
        [HasManyThrough(nameof(WorkItemTags))]
        public ISet<WorkTag> Tags { get; set; }
        public ICollection<WorkItemTag> WorkItemTags { get; set; }

        [HasOne]
        public WorkItemGroup Group { get; set; }
    }
}
