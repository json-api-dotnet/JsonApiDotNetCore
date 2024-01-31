using JsonApiDotNetCoreExampleClient;

#if DEBUG
using var httpClient = new HttpClient(new ColoredConsoleLogDelegatingHandler
{
    InnerHandler = new HttpClientHandler()
});
#else
using var httpClient = new HttpClient();
#endif

var apiClient = new ExampleApiClient(httpClient);

PersonCollectionResponseDocument getResponse = await apiClient.GetPersonCollectionAsync(new Dictionary<string, string?>
{
    ["filter"] = "has(assignedTodoItems)",
    ["sort"] = "-lastName",
    ["page[size]"] = "5"
});

foreach (PersonDataInResponse person in getResponse.Data)
{
    Console.WriteLine($"Found person {person.Id}: {person.Attributes.DisplayName}");
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
    await TranslateAsync(() => apiClient.PatchPersonAsync(patchRequest.Data.Id, null, patchRequest));
}

Console.WriteLine("Press any key to close.");
Console.ReadKey();

// ReSharper disable once UnusedLocalFunctionReturnValue
static async Task<TResponse?> TranslateAsync<TResponse>(Func<Task<TResponse>> operation)
    where TResponse : class
{
    try
    {
        return await operation();
    }
    catch (ApiException exception) when (exception.StatusCode == 204)
    {
        // Workaround for https://github.com/RicoSuter/NSwag/issues/2499
        return null;
    }
}
