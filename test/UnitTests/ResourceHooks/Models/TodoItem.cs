using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.ResourceHooks.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class TodoItem : Identifiable
    {
        [Attr]
        public string Description { get; set; }

        [HasOne]
        public Person Owner { get; set; }

        [HasOne]
        public Person Assignee { get; set; }

        [HasOne]
        public Person OneToOnePerson { get; set; }

        [HasMany]
        public ISet<Person> Stakeholders { get; set; }

        // cyclical to-many structure
        [HasOne]
        public TodoItem ParentTodo { get; set; }

        [HasMany]
        public IList<TodoItem> ChildTodoItems { get; set; }
    }
}
