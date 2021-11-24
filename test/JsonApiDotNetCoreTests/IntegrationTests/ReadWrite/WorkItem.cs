using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ReadWrite")]
    public sealed class WorkItem : Identifiable<int>
    {
        [Attr]
        public string? Description { get; set; }

        [Attr]
        public DateTimeOffset? DueAt { get; set; }

        [Attr]
        public WorkItemPriority Priority { get; set; }

        [NotMapped]
        [Attr(Capabilities = ~(AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange))]
        public bool IsImportant
        {
            get => Priority == WorkItemPriority.High;
            set => Priority = value ? WorkItemPriority.High : throw new NotSupportedException();
        }

        [HasOne]
        public UserAccount? Assignee { get; set; }

        [HasMany]
        public ISet<UserAccount> Subscribers { get; set; } = new HashSet<UserAccount>();

        [HasMany]
        public ISet<WorkTag> Tags { get; set; } = new HashSet<WorkTag>();

        [HasOne]
        public WorkItem? Parent { get; set; }

        [HasMany]
        public IList<WorkItem> Children { get; set; } = new List<WorkItem>();

        [HasMany]
        public IList<WorkItem> RelatedFrom { get; set; } = new List<WorkItem>();

        [HasMany]
        public IList<WorkItem> RelatedTo { get; set; } = new List<WorkItem>();

        [HasOne]
        public WorkItemGroup? Group { get; set; }
    }
}
