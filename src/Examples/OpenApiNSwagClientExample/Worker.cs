using JsonApiDotNetCore.OpenApi.Client.NSwag;

namespace OpenApiNSwagClientExample;

public sealed class Worker(ExampleApiClient apiClient, IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    private readonly ExampleApiClient _apiClient = apiClient;
    private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var queryString = new Dictionary<string, string?>
            {
                ["filter"] = "has(assignedTodoItems)",
                ["sort"] = "-lastName",
                ["page[size]"] = "5",
                ["include"] = "assignedTodoItems.tags"
            };

            ApiResponse<PersonCollectionResponseDocument?> getResponse = await GetPeopleAsync(_apiClient, queryString, null, stoppingToken);
            PeopleMessageFormatter.PrintPeople(getResponse);

            string eTag = getResponse.Headers["ETag"].Single();
            ApiResponse<PersonCollectionResponseDocument?> getResponseAgain = await GetPeopleAsync(_apiClient, queryString, eTag, stoppingToken);
            PeopleMessageFormatter.PrintPeople(getResponseAgain);

            await UpdatePersonAsync(stoppingToken);

            _ = await _apiClient.GetPersonAsync("999999", null, null, stoppingToken);
        }
        catch (ApiException<ErrorResponseDocument> exception)
        {
            Console.WriteLine($"JSON:API ERROR: {exception.Result.Errors.First().Detail}");
        }
        catch (HttpRequestException exception)
        {
            Console.WriteLine($"ERROR: {exception.Message}");
        }

        _hostApplicationLifetime.StopApplication();
    }

    private static Task<ApiResponse<PersonCollectionResponseDocument?>> GetPeopleAsync(ExampleApiClient apiClient, IDictionary<string, string?> queryString,
        string? ifNoneMatch, CancellationToken cancellationToken)
    {
        return ApiResponse.TranslateAsync(() => apiClient.GetPersonCollectionAsync(queryString, ifNoneMatch, cancellationToken));
    }

    private async Task UpdatePersonAsync(CancellationToken cancellationToken)
    {
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
        using (_apiClient.WithPartialAttributeSerialization<PersonPatchRequestDocument, PersonAttributesInPatchRequest>(patchRequest,
            person => person.FirstName))
        {
            _ = await ApiResponse.TranslateAsync(() => _apiClient.PatchPersonAsync(patchRequest.Data.Id, null, patchRequest, cancellationToken));
        }
    }
}
