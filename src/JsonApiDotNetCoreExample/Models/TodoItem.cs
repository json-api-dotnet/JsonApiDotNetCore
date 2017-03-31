using System;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class TodoItem : Identifiable
    {
        public TodoItem()
        {
            GuidProperty = Guid.NewGuid();
        }

        [Attr("description")]
        public string Description { get; set; }

        [Attr("ordinal")]
        public long Ordinal { get; set; }

        [Attr("guid-property")]
        public Guid GuidProperty { get; set; }
        
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
