using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Person : Identifiable, IIsLockable
    {
        public bool IsLocked { get; set; }

        [Attr]
        public string FirstName { get; set; }

        [Attr]
        public string LastName { get; set; }

        [HasMany]
        public ISet<TodoItem> TodoItems { get; set; }

        [HasMany]
        public ISet<TodoItem> AssignedTodoItems { get; set; }

        [HasOne]
        public TodoItem OneToOneTodoItem { get; set; }

        [HasOne]
        public TodoItem StakeHolderTodoItem { get; set; }

        [HasOne]
        public Passport Passport { get; set; }
    }
}
