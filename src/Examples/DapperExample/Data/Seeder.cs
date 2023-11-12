using DapperExample.Models;
using JetBrains.Annotations;

namespace DapperExample.Data;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class Seeder
{
    public static async Task CreateSampleDataAsync(AppDbContext dbContext)
    {
        const int todoItemCount = 500;
        const int personCount = 50;
        const int accountRecoveryCount = 50;
        const int loginAccountCount = 50;
        const int tagCount = 25;
        const int colorCount = 25;

        RotatingList<AccountRecovery> accountRecoveries = RotatingList.Create(accountRecoveryCount, index => new AccountRecovery
        {
            PhoneNumber = $"PhoneNumber{index + 1:D2}",
            EmailAddress = $"EmailAddress{index + 1:D2}"
        });

        RotatingList<LoginAccount> loginAccounts = RotatingList.Create(loginAccountCount, index => new LoginAccount
        {
            UserName = $"UserName{index + 1:D2}",
            Recovery = accountRecoveries.GetNext()
        });

        RotatingList<Person> people = RotatingList.Create(personCount, index =>
        {
            var person = new Person
            {
                FirstName = $"FirstName{index + 1:D2}",
                LastName = $"LastName{index + 1:D2}"
            };

            if (index % 2 == 0)
            {
                person.Account = loginAccounts.GetNext();
            }

            return person;
        });

        RotatingList<RgbColor> colors =
            RotatingList.Create(colorCount, index => RgbColor.Create((byte)(index % 255), (byte)(index % 255), (byte)(index % 255)));

        RotatingList<Tag> tags = RotatingList.Create(tagCount, index =>
        {
            var tag = new Tag
            {
                Name = $"TagName{index + 1:D2}"
            };

            if (index % 2 == 0)
            {
                tag.Color = colors.GetNext();
            }

            return tag;
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
