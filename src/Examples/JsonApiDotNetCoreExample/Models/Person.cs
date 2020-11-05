using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class PersonRole : Identifiable
    {
        [HasOne]
        public Person Person { get; set; }
    }

    public sealed class Person : Identifiable, IIsLockable
    {
        private string _firstName;

        public bool IsLocked { get; set; }

        [Attr]
        public string FirstName
        {
            get => _firstName;
            set
            {
                if (value != _firstName)
                {
                    _firstName = value;
                    Initials = string.Concat(value.Split(' ').Select(x => char.ToUpperInvariant(x[0])));
                }
            }
        }

        [Attr]
        public string Initials { get; set; }

        [Attr]
        public string LastName { get; set; }

        [Attr(PublicName = "the-Age")]
        public int Age { get; set; }

        [Attr]
        public Gender Gender { get; set; }

        [Attr]
        public string Category { get; set; }

        [HasMany]
        public ISet<TodoItem> TodoItems { get; set; }

        [HasMany]
        public ISet<TodoItem> AssignedTodoItems { get; set; }

        [HasMany]
        public HashSet<TodoItemCollection> TodoCollections { get; set; }

        [HasOne]
        public PersonRole Role { get; set; }

        [HasOne]
        public TodoItem OneToOneTodoItem { get; set; }

        [HasOne]
        public TodoItem StakeHolderTodoItem { get; set; }

        [HasOne(Links = LinkTypes.All, CanInclude = false)]
        public TodoItem UnIncludeableItem { get; set; }

        [HasOne]
        public Passport Passport { get; set; }
    }
}
