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

            await SendOperationsRequestAsync(stoppingToken);

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
        return ApiResponse.TranslateAsync(async () => await apiClient.GetPersonCollectionAsync(queryString, ifNoneMatch, cancellationToken));
    }

    private async Task UpdatePersonAsync(CancellationToken cancellationToken)
    {
        var updatePersonRequest = new UpdatePersonRequestDocument
        {
            Data = new DataInUpdatePersonRequest
            {
                Id = "1",
                Attributes = new AttributesInUpdatePersonRequest
                {
                    LastName = "Doe"
                }
            }
        };

        // This line results in sending "firstName: null" instead of omitting it.
        using (_apiClient.WithPartialAttributeSerialization<UpdatePersonRequestDocument, AttributesInUpdatePersonRequest>(updatePersonRequest,
            person => person.FirstName))
        {
            _ = await ApiResponse.TranslateAsync(async () =>
                await _apiClient.PatchPersonAsync(updatePersonRequest.Data.Id, updatePersonRequest, cancellationToken: cancellationToken));
        }
    }

    private async Task SendOperationsRequestAsync(CancellationToken cancellationToken)
    {
        var operationsRequest = new OperationsRequestDocument
        {
            Atomic_operations =
            [
                new CreateTagOperation
                {
                    Data = new DataInCreateTagRequest
                    {
                        Lid = "new-tag",
                        Attributes = new AttributesInCreateTagRequest
                        {
                            Name = "Housekeeping"
                        }
                    }
                },
                new CreatePersonOperation
                {
                    Data = new DataInCreatePersonRequest
                    {
                        Lid = "new-person",
                        Attributes = new AttributesInCreatePersonRequest
                        {
                            LastName = "Cinderella"
                        }
                    }
                },
                new CreateTodoItemOperation
                {
                    Data = new DataInCreateTodoItemRequest
                    {
                        Lid = "new-todo-item",
                        Attributes = new AttributesInCreateTodoItemRequest
                        {
                            Description = "Put out the garbage",
                            Priority = TodoItemPriority.Medium
                        },
                        Relationships = new RelationshipsInCreateTodoItemRequest
                        {
                            Owner = new ToOnePersonInRequest
                            {
                                Data = new PersonIdentifierInRequest
                                {
                                    Lid = "new-person"
                                }
                            },
                            Tags = new ToManyTagInRequest
                            {
                                Data =
                                [
                                    new TagIdentifierInRequest
                                    {
                                        Lid = "new-tag"
                                    }
                                ]
                            }
                        }
                    }
                },
                new UpdateTodoItemAssigneeRelationshipOperation
                {
                    Ref = new TodoItemAssigneeRelationshipIdentifier
                    {
                        Lid = "new-todo-item"
                    },
                    Data = new PersonIdentifierInRequest
                    {
                        Lid = "new-person"
                    }
                }
            ]
        };

        ApiResponse<OperationsResponseDocument> operationsResponse = await _apiClient.PostOperationsAsync(operationsRequest, cancellationToken);

        var newTodoItem = (TodoItemDataInResponse)operationsResponse.Result.Atomic_results.ElementAt(2).Data!;
        Console.WriteLine($"Created todo-item with ID {newTodoItem.Id}: {newTodoItem.Attributes!.Description}.");
    }
}
