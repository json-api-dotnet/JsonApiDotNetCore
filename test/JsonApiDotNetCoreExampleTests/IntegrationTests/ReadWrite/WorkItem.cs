using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ReadWrite
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class WorkItem : Identifiable
    {
        [Attr]
        public string Description { get; set; }

        [Attr]
        public DateTimeOffset? DueAt { get; set; }

        [Attr]
        public WorkItemPriority Priority { get; set; }

        [NotMapped]
        [Attr(Capabilities = ~(AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange))]
        public Guid ConcurrencyToken
        {
            get => Guid.NewGuid();
            set => _ = value;
        }

        [HasOne]
        public UserAccount Assignee { get; set; }

        [HasMany]
        public ISet<UserAccount> Subscribers { get; set; }

        [NotMapped]
        [HasManyThrough(nameof(WorkItemTags))]
        public ISet<WorkTag> Tags { get; set; }

        public ICollection<WorkItemTag> WorkItemTags { get; set; }

        [HasOne]
        public WorkItem Parent { get; set; }

        [HasMany]
        public IList<WorkItem> Children { get; set; }

        [NotMapped]
        [HasManyThrough(nameof(RelatedFromItems), LeftPropertyName = nameof(WorkItemToWorkItem.ToItem),
            RightPropertyName = nameof(WorkItemToWorkItem.FromItem))]
        public IList<WorkItem> RelatedFrom { get; set; }

        public IList<WorkItemToWorkItem> RelatedFromItems { get; set; }

        [NotMapped]
        [HasManyThrough(nameof(RelatedToItems), LeftPropertyName = nameof(WorkItemToWorkItem.FromItem), RightPropertyName = nameof(WorkItemToWorkItem.ToItem))]
        public IList<WorkItem> RelatedTo { get; set; }

        public IList<WorkItemToWorkItem> RelatedToItems { get; set; }

        [HasOne]
        public WorkItemGroup Group { get; set; }
    }
}
