using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite.Updating.Relationships;

public sealed class AddToToManyRelationshipTests : IClassFixture<IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext> _testContext;
    private readonly ReadWriteFakers _fakers = new();

    public AddToToManyRelationshipTests(IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<WorkItemsController>();
    }

    [Fact]
    public async Task Cannot_add_to_ManyToOne_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();
        UserAccount existingUserAccount = _fakers.UserAccount.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingWorkItem, existingUserAccount);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "userAccounts",
                id = existingUserAccount.StringId
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/assignee";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Forbidden);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        error.Title.Should().Be("Failed to deserialize request body: Only to-many relationships can be targeted through this endpoint.");
        error.Detail.Should().Be("Relationship 'assignee' is not a to-many relationship.");
        error.Source.Should().BeNull();
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Can_add_to_OneToMany_relationship_with_already_assigned_resources()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();
        existingWorkItem.Subscribers = _fakers.UserAccount.Generate(2).ToHashSet();

        UserAccount existingSubscriber = _fakers.UserAccount.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingWorkItem, existingSubscriber);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "userAccounts",
                    id = existingWorkItem.Subscribers.ElementAt(1).StringId
                },
                new
                {
                    type = "userAccounts",
                    id = existingSubscriber.StringId
                }
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Subscribers).FirstWithIdAsync(existingWorkItem.Id);

            workItemInDatabase.Subscribers.ShouldHaveCount(3);
            workItemInDatabase.Subscribers.Should().ContainSingle(subscriber => subscriber.Id == existingWorkItem.Subscribers.ElementAt(0).Id);
            workItemInDatabase.Subscribers.Should().ContainSingle(subscriber => subscriber.Id == existingWorkItem.Subscribers.ElementAt(1).Id);
            workItemInDatabase.Subscribers.Should().ContainSingle(subscriber => subscriber.Id == existingSubscriber.Id);
        });
    }

    [Fact]
    public async Task Can_add_to_ManyToMany_relationship_with_already_assigned_resources()
    {
        // Arrange
        List<WorkItem> existingWorkItems = _fakers.WorkItem.Generate(2);
        existingWorkItems[0].Tags = _fakers.WorkTag.Generate(1).ToHashSet();
        existingWorkItems[1].Tags = _fakers.WorkTag.Generate(1).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.AddRange(existingWorkItems);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "workTags",
                    id = existingWorkItems[0].Tags.ElementAt(0).StringId
                },
                new
                {
                    type = "workTags",
                    id = existingWorkItems[1].Tags.ElementAt(0).StringId
                }
            }
        };

        string route = $"/workItems/{existingWorkItems[0].StringId}/relationships/tags";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<WorkItem> workItemsInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Tags).ToListAsync();

            WorkItem workItemInDatabase1 = workItemsInDatabase.Single(workItem => workItem.Id == existingWorkItems[0].Id);

            int tagId1 = existingWorkItems[0].Tags.ElementAt(0).Id;
            int tagId2 = existingWorkItems[1].Tags.ElementAt(0).Id;

            workItemInDatabase1.Tags.ShouldHaveCount(2);
            workItemInDatabase1.Tags.Should().ContainSingle(workTag => workTag.Id == tagId1);
            workItemInDatabase1.Tags.Should().ContainSingle(workTag => workTag.Id == tagId2);

            WorkItem workItemInDatabase2 = workItemsInDatabase.Single(workItem => workItem.Id == existingWorkItems[1].Id);

            workItemInDatabase2.Tags.ShouldHaveCount(1);
            workItemInDatabase2.Tags.ElementAt(0).Id.Should().Be(tagId2);
        });
    }

    [Fact]
    public async Task Cannot_add_for_missing_request_body()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        string requestBody = string.Empty;

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Failed to deserialize request body: Missing request body.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
        error.Meta.Should().NotContainKey("requestBody");
    }

    [Fact]
    public async Task Cannot_add_for_null_request_body()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        const string requestBody = "null";

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Expected an object, instead of 'null'.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_add_for_missing_type()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    id = Unknown.StringId.For<UserAccount, long>()
                }
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'type' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data[0]");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_add_for_unknown_type()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = Unknown.ResourceType,
                    id = Unknown.StringId.For<UserAccount, long>()
                }
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Unknown resource type found.");
        error.Detail.Should().Be($"Resource type '{Unknown.ResourceType}' does not exist.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data[0]/type");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_add_for_missing_ID()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "userAccounts"
                }
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'id' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data[0]");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_add_unknown_IDs_to_OneToMany_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        string userAccountId1 = Unknown.StringId.For<UserAccount, long>();
        string userAccountId2 = Unknown.StringId.AltFor<UserAccount, long>();

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "userAccounts",
                    id = userAccountId1
                },
                new
                {
                    type = "userAccounts",
                    id = userAccountId2
                }
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(2);

        ErrorObject error1 = responseDocument.Errors[0];
        error1.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error1.Title.Should().Be("A related resource does not exist.");
        error1.Detail.Should().Be($"Related resource of type 'userAccounts' with ID '{userAccountId1}' in relationship 'subscribers' does not exist.");
        error1.Meta.Should().NotContainKey("requestBody");

        ErrorObject error2 = responseDocument.Errors[1];
        error2.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error2.Title.Should().Be("A related resource does not exist.");
        error2.Detail.Should().Be($"Related resource of type 'userAccounts' with ID '{userAccountId2}' in relationship 'subscribers' does not exist.");
        error2.Meta.Should().NotContainKey("requestBody");
    }

    [Fact]
    public async Task Cannot_add_unknown_IDs_to_ManyToMany_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        string tagId1 = Unknown.StringId.For<WorkTag, int>();
        string tagId2 = Unknown.StringId.AltFor<WorkTag, int>();

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "workTags",
                    id = tagId1
                },
                new
                {
                    type = "workTags",
                    id = tagId2
                }
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/tags";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(2);

        ErrorObject error1 = responseDocument.Errors[0];
        error1.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error1.Title.Should().Be("A related resource does not exist.");
        error1.Detail.Should().Be($"Related resource of type 'workTags' with ID '{tagId1}' in relationship 'tags' does not exist.");
        error1.Meta.Should().NotContainKey("requestBody");

        ErrorObject error2 = responseDocument.Errors[1];
        error2.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error2.Title.Should().Be("A related resource does not exist.");
        error2.Detail.Should().Be($"Related resource of type 'workTags' with ID '{tagId2}' in relationship 'tags' does not exist.");
        error2.Meta.Should().NotContainKey("requestBody");
    }

    [Fact]
    public async Task Cannot_add_to_unknown_resource_type_in_url()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();
        UserAccount existingSubscriber = _fakers.UserAccount.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingWorkItem, existingSubscriber);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "userAccounts",
                    id = existingSubscriber.StringId
                }
            }
        };

        string route = $"/{Unknown.ResourceType}/{existingWorkItem.StringId}/relationships/subscribers";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Should().BeEmpty();
    }

    [Fact]
    public async Task Cannot_add_to_unknown_resource_ID_in_url()
    {
        // Arrange
        UserAccount existingSubscriber = _fakers.UserAccount.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.UserAccounts.Add(existingSubscriber);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "userAccounts",
                    id = existingSubscriber.StringId
                }
            }
        };

        string workItemId = Unknown.StringId.For<WorkItem, int>();

        string route = $"/workItems/{workItemId}/relationships/subscribers";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'workItems' with ID '{workItemId}' does not exist.");
        error.Source.Should().BeNull();
        error.Meta.Should().NotContainKey("requestBody");
    }

    [Fact]
    public async Task Cannot_add_to_unknown_relationship_in_url()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "userAccounts",
                    id = Unknown.StringId.For<UserAccount, long>()
                }
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/{Unknown.Relationship}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested relationship does not exist.");
        error.Detail.Should().Be($"Resource of type 'workItems' does not contain a relationship named '{Unknown.Relationship}'.");
        error.Source.Should().BeNull();
        error.Meta.Should().NotContainKey("requestBody");
    }

    [Fact]
    public async Task Cannot_add_for_relationship_mismatch_between_url_and_body()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();
        UserAccount existingSubscriber = _fakers.UserAccount.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingWorkItem, existingSubscriber);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "userAccounts",
                    id = existingSubscriber.StringId
                }
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/tags";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().Be("Failed to deserialize request body: Incompatible resource type found.");
        error.Detail.Should().Be("Type 'userAccounts' is not convertible to type 'workTags' of relationship 'tags'.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data[0]/type");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Can_add_with_duplicates()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();
        UserAccount existingSubscriber = _fakers.UserAccount.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingWorkItem, existingSubscriber);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "userAccounts",
                    id = existingSubscriber.StringId
                },
                new
                {
                    type = "userAccounts",
                    id = existingSubscriber.StringId
                }
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Subscribers).FirstWithIdAsync(existingWorkItem.Id);

            workItemInDatabase.Subscribers.ShouldHaveCount(1);
            workItemInDatabase.Subscribers.Single().Id.Should().Be(existingSubscriber.Id);
        });
    }

    [Fact]
    public async Task Can_add_with_empty_list()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = Array.Empty<object>()
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Subscribers).FirstWithIdAsync(existingWorkItem.Id);

            workItemInDatabase.Subscribers.ShouldHaveCount(0);
        });
    }

    [Fact]
    public async Task Cannot_add_with_missing_data_in_OneToMany_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'data' element is required.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_add_with_null_data_in_ManyToMany_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = (object?)null
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/tags";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Expected an array, instead of 'null'.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_add_with_object_data_in_ManyToMany_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/tags";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Expected an array, instead of an object.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Can_add_self_to_cyclic_OneToMany_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();
        existingWorkItem.Children = _fakers.WorkItem.Generate(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "workItems",
                    id = existingWorkItem.StringId
                }
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/children";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Children).FirstWithIdAsync(existingWorkItem.Id);

            workItemInDatabase.Children.ShouldHaveCount(2);
            workItemInDatabase.Children.Should().ContainSingle(workItem => workItem.Id == existingWorkItem.Children[0].Id);
            workItemInDatabase.Children.Should().ContainSingle(workItem => workItem.Id == existingWorkItem.Id);
        });
    }

    [Fact]
    public async Task Can_add_self_to_cyclic_ManyToMany_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();
        existingWorkItem.RelatedTo = _fakers.WorkItem.Generate(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "workItems",
                    id = existingWorkItem.StringId
                }
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/relatedTo";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            WorkItem workItemInDatabase = await dbContext.WorkItems
                .Include(workItem => workItem.RelatedFrom)
                .Include(workItem => workItem.RelatedTo)
                .FirstWithIdAsync(existingWorkItem.Id);

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            workItemInDatabase.RelatedFrom.ShouldHaveCount(1);
            workItemInDatabase.RelatedFrom.Should().OnlyContain(workItem => workItem.Id == existingWorkItem.Id);

            workItemInDatabase.RelatedTo.ShouldHaveCount(2);
            workItemInDatabase.RelatedTo.Should().ContainSingle(workItem => workItem.Id == existingWorkItem.Id);
            workItemInDatabase.RelatedTo.Should().ContainSingle(workItem => workItem.Id == existingWorkItem.RelatedTo[0].Id);
        });
    }
}
