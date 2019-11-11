using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Links;

namespace JsonApiDotNetCoreExample.Models
{
    public class PersonRole : Identifiable
    {
        [HasOne]
        public Person Person { get; set; }
    }

    public class Person : Identifiable, IIsLockable
    {
        public bool IsLocked { get; set; }

        [Attr]
        public string FirstName { get; set; }

        [Attr]
        public string LastName { get; set; }

        [Attr]
        public int Age { get; set; }

        [HasMany]
        public virtual List<TodoItem> TodoItems { get; set; }

        [HasMany]
        public virtual List<TodoItem> AssignedTodoItems { get; set; }

        [HasMany]
        public virtual List<TodoItemCollection> todoCollections { get; set; }

        [HasOne]
        public virtual PersonRole Role { get; set; } 
        public int? PersonRoleId { get; set; }

        [HasOne]
        public virtual TodoItem OneToOneTodoItem { get; set; }

        [HasOne]
        public virtual TodoItem StakeHolderTodoItem { get; set; }
        public virtual int? StakeHolderTodoItemId { get; set; }

        [HasOne(links: Link.All, canInclude: false)]
        public virtual TodoItem UnIncludeableItem { get; set; }

        [HasOne]
        public virtual Passport Passport { get; set; }
        public int? PassportId { get; set; }
    }
}
