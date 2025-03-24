using System.Text;
using JetBrains.Annotations;
using OpenApiKiotaClientExample.GeneratedCode.Models;

namespace OpenApiKiotaClientExample;

/// <summary>
/// Prints the specified people, their assigned todo-items, and its tags.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class PeopleMessageFormatter
{
    public static void PrintPeople(PersonCollectionResponseDocument? peopleResponse)
    {
        string message = WritePeople(peopleResponse);
        Console.WriteLine(message);
    }

    private static string WritePeople(PersonCollectionResponseDocument? peopleResponse)
    {
        if (peopleResponse == null)
        {
            return "No response body was returned.";
        }

        var builder = new StringBuilder();
        builder.AppendLine($"Found {peopleResponse.Data!.Count} people:");

        foreach (DataInPersonResponse person in peopleResponse.Data)
        {
            WritePerson(person, peopleResponse.Included ?? [], builder);
        }

        return builder.ToString();
    }

    private static void WritePerson(DataInPersonResponse person, List<ResourceInResponse> includes, StringBuilder builder)
    {
        List<TodoItemIdentifierInResponse> assignedTodoItems = person.Relationships?.AssignedTodoItems?.Data ?? [];

        builder.AppendLine($"  Person {person.Id}: {person.Attributes?.DisplayName} with {assignedTodoItems.Count} assigned todo-items:");
        WriteRelatedTodoItems(assignedTodoItems, includes, builder);
    }

    private static void WriteRelatedTodoItems(List<TodoItemIdentifierInResponse> todoItemIdentifiers, List<ResourceInResponse> includes, StringBuilder builder)
    {
        foreach (TodoItemIdentifierInResponse todoItemIdentifier in todoItemIdentifiers)
        {
            DataInTodoItemResponse includedTodoItem = includes.OfType<DataInTodoItemResponse>().Single(include => include.Id == todoItemIdentifier.Id);
            List<TagIdentifierInResponse> tags = includedTodoItem.Relationships?.Tags?.Data ?? [];

            builder.AppendLine($"    TodoItem {includedTodoItem.Id}: {includedTodoItem.Attributes?.Description} with {tags.Count} tags:");
            WriteRelatedTags(tags, includes, builder);
        }
    }

    private static void WriteRelatedTags(List<TagIdentifierInResponse> tagIdentifiers, List<ResourceInResponse> includes, StringBuilder builder)
    {
        foreach (TagIdentifierInResponse tagIdentifier in tagIdentifiers)
        {
            DataInTagResponse includedTag = includes.OfType<DataInTagResponse>().Single(include => include.Id == tagIdentifier.Id);
            builder.AppendLine($"      Tag {includedTag.Id}: {includedTag.Attributes?.Name}");
        }
    }
}
