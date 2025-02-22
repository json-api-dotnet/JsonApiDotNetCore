using System.Net;
using JsonApiDotNetCore.OpenApi.Client.Kiota;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;
using OpenApiKiotaClientExample.GeneratedCode;
using OpenApiKiotaClientExample.GeneratedCode.Models;

namespace OpenApiKiotaClientExample;

public sealed class Worker(ExampleApiClient apiClient, IHostApplicationLifetime hostApplicationLifetime, SetQueryStringHttpMessageHandler queryStringHandler)
    : BackgroundService
{
    private readonly ExampleApiClient _apiClient = apiClient;
    private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;
    private readonly SetQueryStringHttpMessageHandler _queryStringHandler = queryStringHandler;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using (_queryStringHandler.CreateScope(new Dictionary<string, string?>
            {
                // Workaround for https://github.com/microsoft/kiota/issues/3800.
                ["filter"] = "has(assignedTodoItems)",
                ["sort"] = "-lastName",
                ["page[size]"] = "5",
                ["include"] = "assignedTodoItems.tags"
            }))
            {
                (PersonCollectionResponseDocument? getResponse, string? eTag) = await GetPeopleAsync(_apiClient, null, stoppingToken);
                PeopleMessageFormatter.PrintPeople(getResponse);

                (PersonCollectionResponseDocument? getResponseAgain, _) = await GetPeopleAsync(_apiClient, eTag, stoppingToken);
                PeopleMessageFormatter.PrintPeople(getResponseAgain);
            }

            await UpdatePersonAsync(stoppingToken);

            await SendOperationsRequestAsync(stoppingToken);

            await _apiClient.Api.People["999999"].GetAsync(cancellationToken: stoppingToken);
        }
        catch (ErrorResponseDocument exception)
        {
            Console.WriteLine($"JSON:API ERROR: {exception.Errors!.First().Detail}");
        }
        catch (HttpRequestException exception)
        {
            Console.WriteLine($"ERROR: {exception.Message}");
        }

        _hostApplicationLifetime.StopApplication();
    }

    private async Task<(PersonCollectionResponseDocument? response, string? eTag)> GetPeopleAsync(ExampleApiClient apiClient, string? ifNoneMatch,
        CancellationToken cancellationToken)
    {
        try
        {
            var headerInspector = new HeadersInspectionHandlerOption
            {
                InspectResponseHeaders = true
            };

            PersonCollectionResponseDocument? responseDocument = await apiClient.Api.People.GetAsync(configuration =>
            {
                if (!string.IsNullOrEmpty(ifNoneMatch))
                {
                    configuration.Headers.Add("If-None-Match", ifNoneMatch);
                }

                configuration.Options.Add(headerInspector);
            }, cancellationToken);

            string eTag = headerInspector.ResponseHeaders["ETag"].Single();

            return (responseDocument, eTag);
        }
        // Workaround for https://github.com/microsoft/kiota/issues/4190.
        catch (ApiException exception) when (exception.ResponseStatusCode == (int)HttpStatusCode.NotModified)
        {
            return (null, null);
        }
    }

    private async Task UpdatePersonAsync(CancellationToken cancellationToken)
    {
        var updatePersonRequest = new UpdatePersonRequestDocument
        {
            Data = new DataInUpdatePersonRequest
            {
                Type = PersonResourceType.People,
                Id = "1",
                Attributes = new AttributesInUpdatePersonRequest
                {
                    // The --backing-store switch enables to send null and default values.
                    FirstName = null,
                    LastName = "Doe"
                }
            }
        };

        await _apiClient.Api.People[updatePersonRequest.Data.Id].PatchAsync(updatePersonRequest, cancellationToken: cancellationToken);
    }

    private async Task SendOperationsRequestAsync(CancellationToken cancellationToken)
    {
        var operationsRequest = new OperationsRequestDocument
        {
            AtomicOperations =
            [
                new CreateTagOperation
                {
                    Op = AddOperationCode.Add,
                    Data = new DataInCreateTagRequest
                    {
                        Type = TagResourceType.Tags,
                        Lid = "new-tag",
                        Attributes = new AttributesInCreateTagRequest
                        {
                            Name = "Housekeeping"
                        }
                    }
                },
                new CreatePersonOperation
                {
                    Op = AddOperationCode.Add,
                    Data = new DataInCreatePersonRequest
                    {
                        Type = PersonResourceType.People,
                        Lid = "new-person",
                        Attributes = new AttributesInCreatePersonRequest
                        {
                            FirstName = "Cinderella",
                            LastName = "Tremaine"
                        }
                    }
                },
                new UpdatePersonOperation
                {
                    Op = UpdateOperationCode.Update,
                    Data = new DataInUpdatePersonRequest
                    {
                        Type = PersonResourceType.People,
                        Lid = "new-person",
                        Attributes = new AttributesInUpdatePersonRequest
                        {
                            // The --backing-store switch enables to send null and default values.
                            FirstName = null
                        }
                    }
                },
                new CreateTodoItemOperation
                {
                    Op = AddOperationCode.Add,
                    Data = new DataInCreateTodoItemRequest
                    {
                        Type = TodoItemResourceType.TodoItems,
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
                                    Type = PersonResourceType.People,
                                    Lid = "new-person"
                                }
                            },
                            Tags = new ToManyTagInRequest
                            {
                                Data =
                                [
                                    new TagIdentifierInRequest
                                    {
                                        Type = TagResourceType.Tags,
                                        Lid = "new-tag"
                                    }
                                ]
                            }
                        }
                    }
                },
                new UpdateTodoItemAssigneeRelationshipOperation
                {
                    Op = UpdateOperationCode.Update,
                    Ref = new TodoItemAssigneeRelationshipIdentifier
                    {
                        Type = TodoItemResourceType.TodoItems,
                        Lid = "new-todo-item",
                        Relationship = TodoItemAssigneeRelationshipName.Assignee
                    },
                    Data = new PersonIdentifierInRequest
                    {
                        Type = PersonResourceType.People,
                        Lid = "new-person"
                    }
                }
            ]
        };

        OperationsResponseDocument? operationsResponse = await _apiClient.Api.Operations.PostAsync(operationsRequest, cancellationToken: cancellationToken);

        var newTodoItem = (TodoItemDataInResponse)operationsResponse!.AtomicResults!.ElementAt(3).Data!;
        Console.WriteLine($"Created todo-item with ID {newTodoItem.Id}: {newTodoItem.Attributes!.Description}.");
    }
}
