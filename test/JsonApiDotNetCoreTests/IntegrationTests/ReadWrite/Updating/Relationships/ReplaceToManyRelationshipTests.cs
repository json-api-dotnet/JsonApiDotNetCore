using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite.Updating.Relationships;

public sealed class ReplaceToManyRelationshipTests : IClassFixture<IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext> _testContext;
    private readonly ReadWriteFakers _fakers = new();

    public ReplaceToManyRelationshipTests(IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<WorkItemsController>();
        testContext.UseController<WorkItemGroupsController>();
    }

    [Fact]
    public async Task Can_clear_OneToMany_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();
        existingWorkItem.Subscribers = _fakers.UserAccount.GenerateSet(2);

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
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Subscribers).FirstWithIdAsync(existingWorkItem.Id);

            workItemInDatabase.Subscribers.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Can_clear_ManyToMany_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();
        existingWorkItem.Tags = _fakers.WorkTag.GenerateSet(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = Array.Empty<object>()
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/tags";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Tags).FirstWithIdAsync(existingWorkItem.Id);

            workItemInDatabase.Tags.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Can_replace_OneToMany_relationship_with_already_assigned_resources()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();
        existingWorkItem.Subscribers = _fakers.UserAccount.GenerateSet(2);

        UserAccount existingSubscriber = _fakers.UserAccount.GenerateOne();

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
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Subscribers).FirstWithIdAsync(existingWorkItem.Id);

            workItemInDatabase.Subscribers.Should().HaveCount(2);
            workItemInDatabase.Subscribers.Should().ContainSingle(userAccount => userAccount.Id == existingWorkItem.Subscribers.ElementAt(1).Id);
            workItemInDatabase.Subscribers.Should().ContainSingle(userAccount => userAccount.Id == existingSubscriber.Id);
        });
    }

    [Fact]
    public async Task Can_replace_ManyToMany_relationship_with_already_assigned_resources()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();
        existingWorkItem.Tags = _fakers.WorkTag.GenerateSet(2);

        List<WorkTag> existingTags = _fakers.WorkTag.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            dbContext.WorkTags.AddRange(existingTags);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "workTags",
                    id = existingWorkItem.Tags.ElementAt(0).StringId
                },
                new
                {
                    type = "workTags",
                    id = existingTags[0].StringId
                },
                new
                {
                    type = "workTags",
                    id = existingTags[1].StringId
                }
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/tags";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Tags).FirstWithIdAsync(existingWorkItem.Id);

            workItemInDatabase.Tags.Should().HaveCount(3);
            workItemInDatabase.Tags.Should().ContainSingle(workTag => workTag.Id == existingWorkItem.Tags.ElementAt(0).Id);
            workItemInDatabase.Tags.Should().ContainSingle(workTag => workTag.Id == existingTags[0].Id);
            workItemInDatabase.Tags.Should().ContainSingle(workTag => workTag.Id == existingTags[1].Id);
        });
    }

    [Fact]
    public async Task Cannot_replace_for_missing_request_body()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        string requestBody = string.Empty;

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        httpResponse.Content.Headers.ContentType.Should().NotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.Default.ToString());

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Failed to deserialize request body: Missing request body.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
        error.Meta.Should().NotContainKey("requestBody");
    }

    [Fact]
    public async Task Cannot_replace_for_null_request_body()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        const string requestBody = "null";

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Expected an object, instead of 'null'.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_replace_for_missing_type()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();

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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'type' element is required.");
        error.Detail.Should().BeNull();
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data[0]");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_replace_for_unknown_type()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();

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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Unknown resource type found.");
        error.Detail.Should().Be($"Resource type '{Unknown.ResourceType}' does not exist.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data[0]/type");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_replace_for_missing_ID()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();

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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'id' element is required.");
        error.Detail.Should().BeNull();
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data[0]");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_replace_with_unknown_IDs_in_OneToMany_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();

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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(2);

        ErrorObject error1 = responseDocument.Errors[0];
        error1.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error1.Title.Should().Be("A related resource does not exist.");
        error1.Detail.Should().Be($"Related resource of type 'userAccounts' with ID '{userAccountId1}' in relationship 'subscribers' does not exist.");
        error1.Source.Should().BeNull();
        error1.Meta.Should().NotContainKey("requestBody");

        ErrorObject error2 = responseDocument.Errors[1];
        error2.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error2.Title.Should().Be("A related resource does not exist.");
        error2.Detail.Should().Be($"Related resource of type 'userAccounts' with ID '{userAccountId2}' in relationship 'subscribers' does not exist.");
        error2.Source.Should().BeNull();
        error2.Meta.Should().NotContainKey("requestBody");
    }

    [Fact]
    public async Task Cannot_replace_with_unknown_IDs_in_ManyToMany_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();

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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(2);

        ErrorObject error1 = responseDocument.Errors[0];
        error1.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error1.Title.Should().Be("A related resource does not exist.");
        error1.Detail.Should().Be($"Related resource of type 'workTags' with ID '{tagId1}' in relationship 'tags' does not exist.");
        error1.Source.Should().BeNull();
        error1.Meta.Should().NotContainKey("requestBody");

        ErrorObject error2 = responseDocument.Errors[1];
        error2.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error2.Title.Should().Be("A related resource does not exist.");
        error2.Detail.Should().Be($"Related resource of type 'workTags' with ID '{tagId2}' in relationship 'tags' does not exist.");
        error2.Source.Should().BeNull();
        error2.Meta.Should().NotContainKey("requestBody");
    }

    [Fact]
    public async Task Cannot_replace_on_unknown_resource_type_in_url()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();
        UserAccount existingSubscriber = _fakers.UserAccount.GenerateOne();

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
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Should().BeEmpty();
    }

    [Fact]
    public async Task Cannot_replace_on_unknown_resource_ID_in_url()
    {
        // Arrange
        UserAccount existingSubscriber = _fakers.UserAccount.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.UserAccounts.Add(existingSubscriber);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = Array.Empty<object>()
        };

        string workItemId = Unknown.StringId.For<WorkItem, int>();

        string route = $"/workItems/{workItemId}/relationships/subscribers";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'workItems' with ID '{workItemId}' does not exist.");
        error.Source.Should().BeNull();
        error.Meta.Should().NotContainKey("requestBody");
    }

    [Fact]
    public async Task Cannot_replace_on_unknown_relationship_in_url()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();

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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested relationship does not exist.");
        error.Detail.Should().Be($"Resource of type 'workItems' does not contain a relationship named '{Unknown.Relationship}'.");
        error.Source.Should().BeNull();
        error.Meta.Should().NotContainKey("requestBody");
    }

    [Fact]
    public async Task Cannot_replace_on_whitespace_relationship_in_url()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();

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

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/%20%20";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested relationship does not exist.");
        error.Detail.Should().Be("Resource of type 'workItems' does not contain a relationship named '  '.");
        error.Source.Should().BeNull();
        error.Meta.Should().NotContainKey("requestBody");
    }

    [Fact]
    public async Task Cannot_replace_on_relationship_mismatch_between_url_and_body()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();
        UserAccount existingSubscriber = _fakers.UserAccount.GenerateOne();

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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().Be("Failed to deserialize request body: Incompatible resource type found.");
        error.Detail.Should().Be("Type 'userAccounts' is not convertible to type 'workTags' of relationship 'tags'.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data[0]/type");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Can_replace_with_duplicates()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();
        existingWorkItem.Subscribers = _fakers.UserAccount.GenerateSet(1);

        UserAccount existingSubscriber = _fakers.UserAccount.GenerateOne();

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
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Subscribers).FirstWithIdAsync(existingWorkItem.Id);

            workItemInDatabase.Subscribers.Should().HaveCount(1);
            workItemInDatabase.Subscribers.Single().Id.Should().Be(existingSubscriber.Id);
        });
    }

    [Fact]
    public async Task Cannot_replace_with_missing_data_in_OneToMany_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();

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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'data' element is required.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_replace_with_null_data_in_ManyToMany_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();

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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Expected an array, instead of 'null'.");
        error.Detail.Should().BeNull();
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_replace_with_object_data_in_ManyToMany_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();

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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Expected an array, instead of an object.");
        error.Detail.Should().BeNull();
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Can_clear_cyclic_OneToMany_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();

            existingWorkItem.Children = new List<WorkItem>
            {
                existingWorkItem
            };

            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = Array.Empty<object>()
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/children";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Children).FirstWithIdAsync(existingWorkItem.Id);

            workItemInDatabase.Children.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Can_clear_cyclic_ManyToMany_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();

            existingWorkItem.RelatedFrom = [existingWorkItem];
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = Array.Empty<object>()
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/relatedFrom";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_after_property_in_chained_method_calls true

            WorkItem workItemInDatabase = await dbContext.WorkItems
                .Include(workItem => workItem.RelatedFrom)
                .Include(workItem => workItem.RelatedTo)
                .FirstWithIdAsync(existingWorkItem.Id);

            // @formatter:wrap_after_property_in_chained_method_calls restore
            // @formatter:wrap_chained_method_calls restore

            workItemInDatabase.RelatedFrom.Should().BeEmpty();
            workItemInDatabase.RelatedTo.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Can_assign_cyclic_OneToMany_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();

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
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Children).FirstWithIdAsync(existingWorkItem.Id);

            workItemInDatabase.Children.Should().HaveCount(1);
            workItemInDatabase.Children[0].Id.Should().Be(existingWorkItem.Id);
        });
    }

    [Fact]
    public async Task Can_assign_cyclic_ManyToMany_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();

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
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_after_property_in_chained_method_calls true

            WorkItem workItemInDatabase = await dbContext.WorkItems
                .Include(workItem => workItem.RelatedFrom)
                .Include(workItem => workItem.RelatedTo)
                .FirstWithIdAsync(existingWorkItem.Id);

            // @formatter:wrap_after_property_in_chained_method_calls restore
            // @formatter:wrap_chained_method_calls restore

            workItemInDatabase.RelatedFrom.Should().HaveCount(1);
            workItemInDatabase.RelatedFrom[0].Id.Should().Be(existingWorkItem.Id);

            workItemInDatabase.RelatedTo.Should().HaveCount(1);
            workItemInDatabase.RelatedTo[0].Id.Should().Be(existingWorkItem.Id);
        });
    }

    [Fact]
    public async Task Cannot_assign_relationship_with_blocked_capability()
    {
        // Arrange
        WorkItemGroup existingWorkItemGroup = _fakers.WorkItemGroup.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Groups.Add(existingWorkItemGroup);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "workItems",
                    id = Unknown.StringId.For<WorkItem, int>()
                }
            }
        };

        string route = $"/workItemGroups/{existingWorkItemGroup.StringId}/relationships/items";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Relationship cannot be assigned.");
        error.Detail.Should().Be("The relationship 'items' on resource type 'workItemGroups' cannot be assigned to.");
        error.Source.Should().BeNull();
        error.Meta.Should().HaveRequestBody();
    }
}
