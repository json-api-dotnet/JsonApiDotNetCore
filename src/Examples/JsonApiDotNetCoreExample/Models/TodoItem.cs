using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class TodoItem : Identifiable, IIsLockable
    {
        public TodoItem()
        {
            GuidProperty = Guid.NewGuid();
        }

        public bool IsLocked { get; set; }

        [Attr]
        public string Description { get; set; }

        [Attr]
        public long Ordinal { get; set; }

        [Attr]
        public Guid GuidProperty { get; set; }

        [Attr]
        public DateTime CreatedDate { get; set; }

        [Attr(isFilterable: false, isSortable: false)]
        public DateTime? AchievedDate { get; set; }

        [Attr]
        public DateTime? UpdatedDate { get; set; }

        [Attr(isImmutable: true)]
        public string CalculatedValue { get => "calculated"; }

        [Attr]
        public DateTimeOffset? OffsetDate { get; set; }
 
        public int? OwnerId { get; set; }
        public int? AssigneeId { get; set; }
        public Guid? CollectionId { get; set; }

        [HasOne]
        public virtual Person Owner { get; set; }

        [HasOne]
        public virtual Person Assignee { get; set; }

        [HasOne]
        public virtual Person OneToOnePerson { get; set; }
        public virtual int? OneToOnePersonId { get; set; }

        [HasMany]
        public virtual List<Person> StakeHolders { get; set; }

        [HasOne]
        public virtual TodoItemCollection Collection { get; set; }

        // cyclical to-one structure
        public virtual int? DependentOnTodoId { get; set; }
        [HasOne]
        public virtual TodoItem DependentOnTodo { get; set; }

        // cyclical to-many structure
        public virtual int? ParentTodoId {get; set;}
        [HasOne]
        public virtual TodoItem ParentTodo { get; set; }
        [HasMany]
        public virtual List<TodoItem> ChildrenTodos { get; set; }
    }
}
