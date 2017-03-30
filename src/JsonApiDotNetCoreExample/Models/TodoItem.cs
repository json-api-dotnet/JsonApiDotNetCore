using System;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class TodoItem : Identifiable
    {
        [Attr("description")]
        public string Description { get; set; }

        [Attr("ordinal")]
        public long Ordinal { get; set; }
        
        public int? OwnerId { get; set; }
        public int? AssigneeId { get; set; }
        public Guid? CollectionId { get; set; }

        [HasOne("owner")]
        public virtual Person Owner { get; set; }

        [HasOne("assignee")]
        public virtual Person Assignee { get; set; }

        [HasOne("collection")]
        public virtual TodoItemCollection Collection { get; set; }
    }
}
