using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class TodoItem : Identifiable
    {
        public TodoItem()
        {
            GuidProperty = Guid.NewGuid();
        }

        public bool IsLocked { get; set; }

        [Attr("description")]
        public string Description { get; set; }

        [Attr("ordinal")]
        public long Ordinal { get; set; }

        [Attr("guid-property")]
        public Guid GuidProperty { get; set; }

        [Attr("created-date")]
        public DateTime CreatedDate { get; set; }

        [Attr("achieved-date", isFilterable: false, isSortable: false)]
        public DateTime? AchievedDate { get; set; }


        [Attr("updated-date")]
        public DateTime? UpdatedDate { get; set; }

        public int? OwnerId { get; set; }
        public int? AssigneeId { get; set; }
        public Guid? CollectionId { get; set; }

        [HasOne("owner")]
        public virtual Person Owner { get; set; }

        [HasOne("assignee")]
        public virtual Person Assignee { get; set; }

        [HasMany("stake-holders")]
        public virtual List<Person> StakeHolders { get; set; }

        [HasOne("collection")]
        public virtual TodoItemCollection Collection { get; set; }


        // cyclical to-one structure
        public virtual int? DependentTodoItemId { get; set; }
        [HasOne("dependent-on-todo")]
        public virtual TodoItem DependentTodoItem { get; set; }


        // cyclical to-many structure
        public virtual int? ParentTodoItemId {get; set;}
        [HasOne("parent-todo")]
        public virtual TodoItem ParentTodoItem { get; set; }
        [HasMany("children-todos")]
        public virtual List<TodoItem> ChildrenTodoItems { get; set; }
    }
}
