using System.Net;
using JsonApiDotNetCore.OpenApi.Client;
using JsonApiDotNetCoreExampleClient;
using Microsoft.Net.Http.Headers;

#if DEBUG
using var httpClient = new HttpClient(new ColoredConsoleLogDelegatingHandler
{
    InnerHandler = new HttpClientHandler()
});
#else
using var httpClient = new HttpClient();
#endif

var apiClient = new ExampleApiClient(httpClient);

ApiResponse<PersonCollectionResponseDocument?> getResponse1 = await GetPersonCollectionAsync(apiClient, null);
ApiResponse<PersonCollectionResponseDocument?> getResponse2 = await GetPersonCollectionAsync(apiClient, getResponse1.Headers[HeaderNames.ETag].First());

if (getResponse2.StatusCode == (int)HttpStatusCode.NotModified)
{
    // getResponse2.Result is null.
}

foreach (PersonDataInResponse person in getResponse1.Result!.Data)
{
    PrintPerson(person, getResponse1.Result.Included);
}

var patchRequest = new PersonPatchRequestDocument
{
    Data = new PersonDataInPatchRequest
    {
        Id = "1",
        Attributes = new PersonAttributesInPatchRequest
        {
            LastName = "Doe"
        }
    }
};

// This line results in sending "firstName: null" instead of omitting it.
using (apiClient.WithPartialAttributeSerialization<PersonPatchRequestDocument, PersonAttributesInPatchRequest>(patchRequest, person => person.FirstName))
{
    // Workaround for https://github.com/RicoSuter/NSwag/issues/2499.
    await ApiResponse.TranslateAsync(() => apiClient.PatchPersonAsync(patchRequest.Data.Id, null, patchRequest));
}

Console.WriteLine("Press any key to close.");
Console.ReadKey();

static Task<ApiResponse<PersonCollectionResponseDocument?>> GetPersonCollectionAsync(ExampleApiClient apiClient, string? ifNoneMatch)
{
    return ApiResponse.TranslateAsync(() => apiClient.GetPersonCollectionAsync(new Dictionary<string, string?>
    {
        ["filter"] = "has(assignedTodoItems)",
        ["sort"] = "-lastName",
        ["page[size]"] = "5",
        ["include"] = "assignedTodoItems.tags"
    }, ifNoneMatch));
}

static void PrintPerson(PersonDataInResponse person, ICollection<DataInResponse> includes)
{
    ToManyTodoItemInResponse assignedTodoItems = person.Relationships.AssignedTodoItems;

    Console.WriteLine($"Found person {person.Id}: {person.Attributes.DisplayName} with {assignedTodoItems.Data.Count} assigned todo-items:");

    PrintRelatedTodoItems(assignedTodoItems.Data, includes);
}

static void PrintRelatedTodoItems(IEnumerable<TodoItemIdentifier> todoItemIdentifiers, ICollection<DataInResponse> includes)
{
    foreach (TodoItemIdentifier todoItemIdentifier in todoItemIdentifiers)
    {
        TodoItemDataInResponse includedTodoItem = includes.OfType<TodoItemDataInResponse>().Single(include => include.Id == todoItemIdentifier.Id);
        Console.WriteLine($"  TodoItem {includedTodoItem.Id}: {includedTodoItem.Attributes.Description}");

        PrintRelatedTags(includedTodoItem.Relationships.Tags.Data, includes);
    }
}

static void PrintRelatedTags(IEnumerable<TagIdentifier> tagIdentifiers, ICollection<DataInResponse> includes)
{
    foreach (TagIdentifier tagIdentifier in tagIdentifiers)
    {
        TagDataInResponse includedTag = includes.OfType<TagDataInResponse>().Single(include => include.Id == tagIdentifier.Id);
        Console.WriteLine($"    Tag  {includedTag.Id}: {includedTag.Attributes.Name}");
    }
}
