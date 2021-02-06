using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    public class TodoItem : Identifiable, IIsLockable
    {
        public bool IsLocked { get; set; }

        [Attr]
        public string Description { get; set; }

        [HasOne]
        public Person Owner { get; set; }

        [HasOne]
        public Person Assignee { get; set; }

        [HasOne]
        public Person OneToOnePerson { get; set; }

        [HasMany]
        public ISet<Person> StakeHolders { get; set; }

        // cyclical to-many structure
        [HasOne]
        public TodoItem ParentTodo { get; set; }

        [HasMany]
        public IList<TodoItem> ChildrenTodos { get; set; }
    }
}
