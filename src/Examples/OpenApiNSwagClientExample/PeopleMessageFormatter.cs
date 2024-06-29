using System.Text;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Client.NSwag;

namespace OpenApiNSwagClientExample;

/// <summary>
/// Prints the specified people, their assigned todo-items, and its tags.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class PeopleMessageFormatter
{
    public static void PrintPeople(ApiResponse<PersonCollectionResponseDocument?> peopleResponse)
    {
        string message = WritePeople(peopleResponse);
        Console.WriteLine(message);
    }

    private static string WritePeople(ApiResponse<PersonCollectionResponseDocument?> peopleResponse)
    {
        if (peopleResponse.Result == null)
        {
            return $"Status code {peopleResponse.StatusCode} was returned without a response body.";
        }

        var builder = new StringBuilder();
        builder.AppendLine($"Found {peopleResponse.Result.Data.Count} people:");

        foreach (PersonDataInResponse person in peopleResponse.Result.Data)
        {
            WritePerson(person, peopleResponse.Result.Included ?? [], builder);
        }

        return builder.ToString();
    }

    private static void WritePerson(PersonDataInResponse person, ICollection<DataInResponse> includes, StringBuilder builder)
    {
        ICollection<TodoItemIdentifierInResponse> assignedTodoItems = person.Relationships?.AssignedTodoItems?.Data ?? [];

        builder.AppendLine($"  Person {person.Id}: {person.Attributes?.DisplayName} with {assignedTodoItems.Count} assigned todo-items:");
        WriteRelatedTodoItems(assignedTodoItems, includes, builder);
    }

    private static void WriteRelatedTodoItems(IEnumerable<TodoItemIdentifierInResponse> todoItemIdentifiers, ICollection<DataInResponse> includes,
        StringBuilder builder)
    {
        foreach (TodoItemIdentifierInResponse todoItemIdentifier in todoItemIdentifiers)
        {
            TodoItemDataInResponse includedTodoItem = includes.OfType<TodoItemDataInResponse>().Single(include => include.Id == todoItemIdentifier.Id);
            ICollection<TagIdentifierInResponse> tags = includedTodoItem.Relationships?.Tags?.Data ?? [];

            builder.AppendLine($"    TodoItem {includedTodoItem.Id}: {includedTodoItem.Attributes?.Description} with {tags.Count} tags:");
            WriteRelatedTags(tags, includes, builder);
        }
    }

    private static void WriteRelatedTags(IEnumerable<TagIdentifierInResponse> tagIdentifiers, ICollection<DataInResponse> includes, StringBuilder builder)
    {
        foreach (TagIdentifierInResponse tagIdentifier in tagIdentifiers)
        {
            TagDataInResponse includedTag = includes.OfType<TagDataInResponse>().Single(include => include.Id == tagIdentifier.Id);
            builder.AppendLine($"      Tag {includedTag.Id}: {includedTag.Attributes?.Name}");
        }
    }
}
