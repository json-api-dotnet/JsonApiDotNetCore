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
        public List<TodoItem> TodoItems { get; set; }

        [HasMany]
        public List<TodoItem> AssignedTodoItems { get; set; }

        [HasMany]
        public List<TodoItemCollection> todoCollections { get; set; }

        [HasOne]
        public PersonRole Role { get; set; }
        public int? PersonRoleId { get; set; }

        [HasOne]
        public TodoItem OneToOneTodoItem { get; set; }

        [HasOne]
        public TodoItem StakeHolderTodoItem { get; set; }
        public int? StakeHolderTodoItemId { get; set; }

        [HasOne(links: Link.All, canInclude: false)]
        public TodoItem UnIncludeableItem { get; set; }

        [HasOne]
        public Passport Passport { get; set; }
        public int? PassportId { get; set; }
    }
}
