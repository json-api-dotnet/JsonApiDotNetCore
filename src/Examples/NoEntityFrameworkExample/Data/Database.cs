using JetBrains.Annotations;
using NoEntityFrameworkExample.Models;

namespace NoEntityFrameworkExample.Data;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class Database
{
    public static List<TodoItem> TodoItems { get; }
    public static List<Tag> Tags { get; }
    public static List<Person> People { get; }

    static Database()
    {
        int personIndex = 0;
        int tagIndex = 0;
        int todoItemIndex = 0;

        var john = new Person
        {
            Id = ++personIndex,
            FirstName = "John",
            LastName = "Doe"
        };

        var jane = new Person
        {
            Id = ++personIndex,
            FirstName = "Jane",
            LastName = "Doe"
        };

        var personalTag = new Tag
        {
            Id = ++tagIndex,
            Name = "Personal"
        };

        var familyTag = new Tag
        {
            Id = ++tagIndex,
            Name = "Family"
        };

        var businessTag = new Tag
        {
            Id = ++tagIndex,
            Name = "Business"
        };

        TodoItems = new List<TodoItem>
        {
            new()
            {
                Id = ++todoItemIndex,
                Description = "Make homework",
                DurationInHours = 3,
                Priority = TodoItemPriority.High,
                Owner = john,
                Assignee = jane,
                Tags =
                {
                    personalTag
                }
            },
            new()
            {
                Id = ++todoItemIndex,
                Description = "Book vacation",
                DurationInHours = 2,
                Priority = TodoItemPriority.Low,
                Owner = jane,
                Tags =
                {
                    personalTag
                }
            },
            new()
            {
                Id = ++todoItemIndex,
                Description = "Cook dinner",
                DurationInHours = 1,
                Priority = TodoItemPriority.Medium,
                Owner = jane,
                Assignee = john,
                Tags =
                {
                    familyTag,
                    personalTag
                }
            },
            new()
            {
                Id = ++todoItemIndex,
                Description = "Check emails",
                DurationInHours = 1,
                Priority = TodoItemPriority.Low,
                Owner = john,
                Assignee = john,
                Tags =
                {
                    businessTag
                }
            }
        };

        Tags = new List<Tag>
        {
            personalTag,
            familyTag,
            businessTag
        };

        People = new List<Person>
        {
            john,
            jane
        };

        foreach (Tag tag in Tags)
        {
            tag.TodoItems = TodoItems.Where(todoItem => todoItem.Tags.Any(tagInTodoItem => tagInTodoItem.Id == tag.Id)).ToHashSet();
        }

        foreach (Person person in People)
        {
            person.OwnedTodoItems = TodoItems.Where(todoItem => todoItem.Owner == person).ToHashSet();
            person.AssignedTodoItems = TodoItems.Where(todoItem => todoItem.Assignee == person).ToHashSet();
        }
    }
}
