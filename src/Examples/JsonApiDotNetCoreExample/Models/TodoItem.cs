using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

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
        public string AlwaysChangingValue
        {
            get => Guid.NewGuid().ToString();
            set { }
        }

        [Attr]
        public DateTime CreatedDate { get; set; }

        [Attr(AttrCapabilities.All & ~(AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort))]
        public DateTime? AchievedDate { get; set; }

        [Attr]
        public DateTime? UpdatedDate { get; set; }

        [Attr(AttrCapabilities.All & ~AttrCapabilities.AllowChange)]
        public string CalculatedValue => "calculated";

        [Attr]
        public DateTimeOffset? OffsetDate { get; set; }
 
        public int? OwnerId { get; set; }

        public int? AssigneeId { get; set; }

        public Guid? CollectionId { get; set; }

        [HasOne]
        public Person Owner { get; set; }

        [HasOne]
        public Person Assignee { get; set; }

        [HasOne]
        public Person OneToOnePerson { get; set; }

        public int? OneToOnePersonId { get; set; }

        [HasMany]
        public ISet<Person> StakeHolders { get; set; }

        [HasOne]
        public TodoItemCollection Collection { get; set; }

        // cyclical to-one structure
        public int? DependentOnTodoId { get; set; }

        [HasOne]
        public TodoItem DependentOnTodo { get; set; }

        // cyclical to-many structure
        public int? ParentTodoId {get; set;}

        [HasOne]
        public TodoItem ParentTodo { get; set; }

        [HasMany]
        public IList<TodoItem> ChildrenTodos { get; set; }
    }
}
