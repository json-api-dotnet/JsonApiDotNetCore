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

        foreach (DataInPersonResponse person in peopleResponse.Result.Data)
        {
            WritePerson(person, peopleResponse.Result.Included ?? [], builder);
        }

        return builder.ToString();
    }

    private static void WritePerson(DataInPersonResponse person, ICollection<ResourceInResponse> includes, StringBuilder builder)
    {
        ICollection<TodoItemIdentifierInResponse> assignedTodoItems = person.Relationships.AssignedTodoItems?.Data ?? [];

        builder.AppendLine($"  Person {person.Id}: {person.Attributes.DisplayName ?? string.Empty} with {assignedTodoItems.Count} assigned todo-items:");
        WriteRelatedTodoItems(assignedTodoItems, includes, builder);
    }

    private static void WriteRelatedTodoItems(IEnumerable<TodoItemIdentifierInResponse> todoItemIdentifiers, ICollection<ResourceInResponse> includes,
        StringBuilder builder)
    {
        foreach (TodoItemIdentifierInResponse todoItemIdentifier in todoItemIdentifiers)
        {
            DataInTodoItemResponse includedTodoItem = includes.OfType<DataInTodoItemResponse>().Single(include => include.Id == todoItemIdentifier.Id);
            ICollection<TagIdentifierInResponse> tags = includedTodoItem.Relationships?.Tags?.Data ?? [];

            builder.AppendLine($"    TodoItem {includedTodoItem.Id}: {includedTodoItem.Attributes.Description ?? string.Empty} with {tags.Count} tags:");
            WriteRelatedTags(tags, includes, builder);
        }
    }

    private static void WriteRelatedTags(IEnumerable<TagIdentifierInResponse> tagIdentifiers, ICollection<ResourceInResponse> includes, StringBuilder builder)
    {
        foreach (TagIdentifierInResponse tagIdentifier in tagIdentifiers)
        {
            DataInTagResponse includedTag = includes.OfType<DataInTagResponse>().Single(include => include.Id == tagIdentifier.Id);
            builder.AppendLine($"      Tag {includedTag.Id}: {includedTag.Attributes?.Name ?? string.Empty}");
        }
    }
}
