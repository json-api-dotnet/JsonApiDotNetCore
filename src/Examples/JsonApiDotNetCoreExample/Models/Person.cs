using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Links;

namespace JsonApiDotNetCoreExample.Models
{
    public class PersonRole : Identifiable
    {
        [HasOne("person")]
        public Person Person { get; set; }
    }

    public class Person : Identifiable, IIsLockable
    {
        public bool IsLocked { get; set; }

        [Attr("first-name")]
        public string FirstName { get; set; }

        [Attr("last-name")]
        public string LastName { get; set; }

        [Attr("age")]
        public int Age { get; set; }

        [HasMany("todo-items")]
        public virtual List<TodoItem> TodoItems { get; set; }

        [HasMany("assigned-todo-items")]
        public virtual List<TodoItem> AssignedTodoItems { get; set; }

        [HasMany("todo-collections")]
        public virtual List<TodoItemCollection> TodoItemCollections { get; set; }

        [HasOne("role")]
        public virtual PersonRole Role { get; set; } 
        public int? PersonRoleId { get; set; }

        [HasOne("one-to-one-todo-item")]
        public virtual TodoItem ToOneTodoItem { get; set; }


        [HasOne("stake-holder-todo-item")]
        public virtual TodoItem StakeHolderTodo { get; set; }
        public virtual int? StakeHolderTodoId { get; set; }

        [HasOne("unincludeable-item", links: Link.All, canInclude: false)]
        public virtual TodoItem UnIncludeableItem { get; set; }

        public int? PassportId { get; set; }

        [HasOne("passport")]
        public virtual Passport Passport { get; set; }

    }
}
