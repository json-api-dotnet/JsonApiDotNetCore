using JetBrains.Annotations;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Data;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class Seeder
{
    public static async Task CreateSampleDataAsync(AppDbContext dbContext)
    {
        const int todoItemCount = 500;
        const int personCount = 50;
        const int tagCount = 25;

        RotatingList<Person> people = RotatingList.Create(personCount, index => new Person
        {
            FirstName = $"FirstName{index + 1:D2}",
            LastName = $"LastName{index + 1:D2}"
        });

        RotatingList<Tag> tags = RotatingList.Create(tagCount, index => new Tag
        {
            Name = $"TagName{index + 1:D2}"
        });

        RotatingList<TodoItemPriority> priorities = RotatingList.Create(3, index => (TodoItemPriority)(index + 1));

        RotatingList<TodoItem> todoItems = RotatingList.Create(todoItemCount, index =>
        {
            var todoItem = new TodoItem
            {
                Description = $"TodoItem{index + 1:D3}",
                Priority = priorities.GetNext(),
                DurationInHours = index,
                CreatedAt = DateTimeOffset.UtcNow,
                Owner = people.GetNext(),
                Tags = new HashSet<Tag>
                {
                    tags.GetNext(),
                    tags.GetNext(),
                    tags.GetNext()
                }
            };

            if (index % 3 == 0)
            {
                todoItem.Assignee = people.GetNext();
            }

            return todoItem;
        });

        dbContext.TodoItems.AddRange(todoItems.Elements);
        await dbContext.SaveChangesAsync();
    }
}
