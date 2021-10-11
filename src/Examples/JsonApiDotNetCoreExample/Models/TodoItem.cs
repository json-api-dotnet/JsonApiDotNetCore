#nullable disable

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class TodoItem : Identifiable<int>
    {
        [Attr]
        public string Description { get; set; }

        [Attr]
        public TodoItemPriority Priority { get; set; }

        [Attr(Capabilities = AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort | AttrCapabilities.AllowView)]
        public DateTimeOffset CreatedAt { get; set; }

        [Attr(PublicName = "modifiedAt", Capabilities = AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort | AttrCapabilities.AllowView)]
        public DateTimeOffset? LastModifiedAt { get; set; }

        [HasOne]
        public Person Owner { get; set; }

        [HasOne]
        public Person Assignee { get; set; }

        [HasMany]
        public ISet<Tag> Tags { get; set; }
    }
}
