using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.ResourceHooks.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Person : Identifiable
    {
        [Attr]
        public string Name { get; set; }

        [HasMany]
        public ISet<TodoItem> TodoItems { get; set; }

        [HasMany]
        public ISet<TodoItem> AssignedTodoItems { get; set; }

        [HasOne]
        public TodoItem OneToOneTodoItem { get; set; }

        [HasOne]
        public TodoItem StakeholderTodoItem { get; set; }

        [HasOne]
        public Passport Passport { get; set; }
    }
}
