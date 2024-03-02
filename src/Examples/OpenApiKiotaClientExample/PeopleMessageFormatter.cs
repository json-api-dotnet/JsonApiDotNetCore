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
            return "The HTTP response hasn't changed, so no response body was returned.";
        }

        var builder = new StringBuilder();
        builder.AppendLine($"Found {peopleResponse.Data!.Count} people:");

        foreach (PersonDataInResponse person in peopleResponse.Data)
        {
            WritePerson(person, peopleResponse.Included!, builder);
        }

        return builder.ToString();
    }

    private static void WritePerson(PersonDataInResponse person, ICollection<DataInResponse> includes, StringBuilder builder)
    {
        ToManyTodoItemInResponse assignedTodoItems = person.Relationships!.AssignedTodoItems!;

        builder.AppendLine($"  Person {person.Id}: {person.Attributes!.DisplayName} with {assignedTodoItems.Data!.Count} assigned todo-items:");
        WriteRelatedTodoItems(assignedTodoItems.Data, includes, builder);
    }

    private static void WriteRelatedTodoItems(IEnumerable<TodoItemIdentifier> todoItemIdentifiers, ICollection<DataInResponse> includes, StringBuilder builder)
    {
        foreach (TodoItemIdentifier todoItemIdentifier in todoItemIdentifiers)
        {
            TodoItemDataInResponse includedTodoItem = includes.OfType<TodoItemDataInResponse>().Single(include => include.Id == todoItemIdentifier.Id);
            ToManyTagInResponse tags = includedTodoItem.Relationships!.Tags!;

            builder.AppendLine($"    TodoItem {includedTodoItem.Id}: {includedTodoItem.Attributes!.Description} with {tags.Data!.Count} tags:");
            WriteRelatedTags(tags.Data, includes, builder);
        }
    }

    private static void WriteRelatedTags(IEnumerable<TagIdentifier> tagIdentifiers, ICollection<DataInResponse> includes, StringBuilder builder)
    {
        foreach (TagIdentifier tagIdentifier in tagIdentifiers)
        {
            TagDataInResponse includedTag = includes.OfType<TagDataInResponse>().Single(include => include.Id == tagIdentifier.Id);
            builder.AppendLine($"      Tag {includedTag.Id}: {includedTag.Attributes!.Name}");
        }
    }
}
