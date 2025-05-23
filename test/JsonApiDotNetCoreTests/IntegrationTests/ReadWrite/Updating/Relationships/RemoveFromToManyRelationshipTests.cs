using System.Net;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite.Updating.Relationships;

public sealed class RemoveFromToManyRelationshipTests : IClassFixture<IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext> _testContext;
    private readonly ReadWriteFakers _fakers = new();

    public RemoveFromToManyRelationshipTests(IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<WorkItemsController>();
        testContext.UseController<WorkItemGroupsController>();

        testContext.ConfigureServices(services => services.AddSingleton<IResourceDefinition<WorkItem, int>, RemoveExtraFromWorkItemDefinition>());

        var workItemDefinition = (RemoveExtraFromWorkItemDefinition)testContext.Factory.Services.GetRequiredService<IResourceDefinition<WorkItem, int>>();
        workItemDefinition.Reset();
    }

    [Fact]
    public async Task Cannot_remove_from_ManyToOne_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();
        existingWorkItem.Assignee = _fakers.UserAccount.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "userAccounts",
                id = existingWorkItem.Assignee.StringId
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/assignee";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Forbidden);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        error.Title.Should().Be("Failed to deserialize request body: Only to-many relationships can be targeted through this endpoint.");
        error.Detail.Should().Be("Relationship 'assignee' is not a to-many relationship.");
        error.Source.Should().BeNull();
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Can_remove_from_OneToMany_relationship_with_unassigned_existing_resource()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();
        existingWorkItem.Subscribers = _fakers.UserAccount.GenerateSet(2);
        UserAccount existingSubscriber = _fakers.UserAccount.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<UserAccount>();
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
                    id = existingWorkItem.Subscribers.ElementAt(0).StringId
                }
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Subscribers).FirstWithIdAsync(existingWorkItem.Id);

            workItemInDatabase.Subscribers.Should().HaveCount(1);
            workItemInDatabase.Subscribers.Single().Id.Should().Be(existingWorkItem.Subscribers.ElementAt(1).Id);

            List<UserAccount> userAccountsInDatabase = await dbContext.UserAccounts.ToListAsync();
            userAccountsInDatabase.Should().HaveCount(3);
        });
    }

    [Fact]
    public async Task Can_remove_from_OneToMany_relationship_with_extra_removals_from_resource_definition()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();
        existingWorkItem.Subscribers = _fakers.UserAccount.GenerateSet(3);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<UserAccount>();
            dbContext.AddInRange(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        var workItemDefinition = (RemoveExtraFromWorkItemDefinition)_testContext.Factory.Services.GetRequiredService<IResourceDefinition<WorkItem, int>>();
        workItemDefinition.ExtraSubscribersIdsToRemove.Add(existingWorkItem.Subscribers.ElementAt(2).Id);

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "userAccounts",
                    id = existingWorkItem.Subscribers.ElementAt(0).StringId
                }
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        workItemDefinition.PreloadedSubscribers.Should().HaveCount(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Subscribers).FirstWithIdAsync(existingWorkItem.Id);

            workItemInDatabase.Subscribers.Should().HaveCount(1);
            workItemInDatabase.Subscribers.Single().Id.Should().Be(existingWorkItem.Subscribers.ElementAt(1).Id);

            List<UserAccount> userAccountsInDatabase = await dbContext.UserAccounts.ToListAsync();
            userAccountsInDatabase.Should().HaveCount(3);
        });
    }

    [Fact]
    public async Task Can_remove_from_ManyToMany_relationship_with_unassigned_existing_resource()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();
        existingWorkItem.Tags = _fakers.WorkTag.GenerateSet(2);

        WorkTag existingTag = _fakers.WorkTag.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<WorkTag>();
            dbContext.AddInRange(existingWorkItem, existingTag);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "workTags",
                    id = existingWorkItem.Tags.ElementAt(1).StringId
                },
                new
                {
                    type = "workTags",
                    id = existingTag.StringId
                }
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/tags";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Tags).FirstWithIdAsync(existingWorkItem.Id);

            workItemInDatabase.Tags.Should().HaveCount(1);
            workItemInDatabase.Tags.Single().Id.Should().Be(existingWorkItem.Tags.ElementAt(0).Id);

            List<WorkTag> tagsInDatabase = await dbContext.WorkTags.ToListAsync();
            tagsInDatabase.Should().HaveCount(3);
        });
    }

    [Fact]
    public async Task Can_remove_from_ManyToMany_relationship_with_extra_removals_from_resource_definition()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();
        existingWorkItem.Tags = _fakers.WorkTag.GenerateSet(3);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<WorkTag>();
            dbContext.AddInRange(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        var workItemDefinition = (RemoveExtraFromWorkItemDefinition)_testContext.Factory.Services.GetRequiredService<IResourceDefinition<WorkItem, int>>();
        workItemDefinition.ExtraTagIdsToRemove.Add(existingWorkItem.Tags.ElementAt(2).Id);

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "workTags",
                    id = existingWorkItem.Tags.ElementAt(1).StringId
                }
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/tags";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        workItemDefinition.PreloadedTags.Should().HaveCount(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Tags).FirstWithIdAsync(existingWorkItem.Id);

            workItemInDatabase.Tags.Should().HaveCount(1);
            workItemInDatabase.Tags.Single().Id.Should().Be(existingWorkItem.Tags.ElementAt(0).Id);

            List<WorkTag> tagsInDatabase = await dbContext.WorkTags.ToListAsync();
            tagsInDatabase.Should().HaveCount(3);
        });
    }

    [Fact]
    public async Task Cannot_remove_for_missing_request_body()
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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

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
    public async Task Cannot_remove_for_null_request_body()
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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

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
    public async Task Cannot_remove_for_missing_type()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();
        existingWorkItem.Subscribers = _fakers.UserAccount.GenerateSet(1);

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
                    id = existingWorkItem.Subscribers.ElementAt(0).StringId
                }
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

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
    public async Task Cannot_remove_for_unknown_type()
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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

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
    public async Task Cannot_remove_for_missing_ID()
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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

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
    public async Task Cannot_remove_unknown_IDs_from_OneToMany_relationship()
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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(2);

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
    public async Task Cannot_remove_unknown_IDs_from_ManyToMany_relationship()
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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(2);

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
    public async Task Cannot_remove_from_unknown_resource_type_in_url()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();
        existingWorkItem.Subscribers = _fakers.UserAccount.GenerateSet(1);

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
                    id = existingWorkItem.Subscribers.ElementAt(0).StringId
                }
            }
        };

        string route = $"/{Unknown.ResourceType}/{existingWorkItem.StringId}/relationships/subscribers";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Should().BeEmpty();
    }

    [Fact]
    public async Task Cannot_remove_from_unknown_resource_ID_in_url()
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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

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
    public async Task Cannot_remove_from_unknown_relationship_in_url()
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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

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
    public async Task Cannot_remove_from_whitespace_relationship_in_url()
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

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/%20";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested relationship does not exist.");
        error.Detail.Should().Be("Resource of type 'workItems' does not contain a relationship named ' '.");
        error.Source.Should().BeNull();
        error.Meta.Should().NotContainKey("requestBody");
    }

    [Fact]
    public async Task Cannot_remove_for_relationship_mismatch_between_url_and_body()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();
        existingWorkItem.Subscribers = _fakers.UserAccount.GenerateSet(1);

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
                    id = existingWorkItem.Subscribers.ElementAt(0).StringId
                }
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/tags";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

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
    public async Task Can_remove_with_duplicates()
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
            data = new[]
            {
                new
                {
                    type = "userAccounts",
                    id = existingWorkItem.Subscribers.ElementAt(0).StringId
                },
                new
                {
                    type = "userAccounts",
                    id = existingWorkItem.Subscribers.ElementAt(0).StringId
                }
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Subscribers).FirstWithIdAsync(existingWorkItem.Id);

            workItemInDatabase.Subscribers.Should().HaveCount(1);
            workItemInDatabase.Subscribers.Single().Id.Should().Be(existingWorkItem.Subscribers.ElementAt(1).Id);
        });
    }

    [Fact]
    public async Task Can_remove_with_empty_list()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();
        existingWorkItem.Subscribers = _fakers.UserAccount.GenerateSet(1);

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
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Subscribers).FirstWithIdAsync(existingWorkItem.Id);

            workItemInDatabase.Subscribers.Should().HaveCount(1);
            workItemInDatabase.Subscribers.Single().Id.Should().Be(existingWorkItem.Subscribers.ElementAt(0).Id);
        });
    }

    [Fact]
    public async Task Cannot_remove_with_missing_data_in_OneToMany_relationship()
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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

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
    public async Task Cannot_remove_with_null_data_in_ManyToMany_relationship()
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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

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
    public async Task Cannot_remove_with_object_data_in_ManyToMany_relationship()
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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

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
    public async Task Can_remove_self_from_cyclic_OneToMany_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();
        existingWorkItem.Children = _fakers.WorkItem.GenerateList(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();

            existingWorkItem.Children.Add(existingWorkItem);
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
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Children).FirstWithIdAsync(existingWorkItem.Id);

            workItemInDatabase.Children.Should().HaveCount(1);
            workItemInDatabase.Children[0].Id.Should().Be(existingWorkItem.Children[0].Id);
        });
    }

    [Fact]
    public async Task Can_remove_self_from_cyclic_ManyToMany_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();
        existingWorkItem.RelatedFrom = _fakers.WorkItem.GenerateList(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();

            existingWorkItem.RelatedFrom.Add(existingWorkItem);
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

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/relatedFrom";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

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
            workItemInDatabase.RelatedFrom[0].Id.Should().Be(existingWorkItem.RelatedFrom[0].Id);

            workItemInDatabase.RelatedTo.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Cannot_remove_with_blocked_capability()
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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Relationship cannot be removed from.");
        error.Detail.Should().Be("The relationship 'items' on resource type 'workItemGroups' cannot be removed from.");
        error.Source.Should().BeNull();
        error.Meta.Should().HaveRequestBody();
    }

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    private sealed class RemoveExtraFromWorkItemDefinition(IResourceGraph resourceGraph)
        : JsonApiResourceDefinition<WorkItem, int>(resourceGraph)
    {
        // Enables to verify that not the full relationship was loaded upfront.
        public HashSet<UserAccount> PreloadedSubscribers { get; } = new(IdentifiableComparer.Instance);
        public HashSet<WorkTag> PreloadedTags { get; } = new(IdentifiableComparer.Instance);

        // Enables to verify that adding extra IDs for removal from ResourceDefinition works correctly.
        public HashSet<long> ExtraSubscribersIdsToRemove { get; } = [];
        public HashSet<int> ExtraTagIdsToRemove { get; } = [];

        public void Reset()
        {
            PreloadedSubscribers.Clear();
            PreloadedTags.Clear();

            ExtraSubscribersIdsToRemove.Clear();
            ExtraTagIdsToRemove.Clear();
        }

        public override Task OnRemoveFromRelationshipAsync(WorkItem workItem, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
            CancellationToken cancellationToken)
        {
            if (hasManyRelationship.Property.Name == nameof(WorkItem.Subscribers))
            {
                RemoveFromSubscribers(workItem, rightResourceIds);
            }
            else if (hasManyRelationship.Property.Name == nameof(WorkItem.Tags))
            {
                RemoveFromTags(workItem, rightResourceIds);
            }

            return Task.CompletedTask;
        }

        private void RemoveFromSubscribers(WorkItem workItem, ISet<IIdentifiable> rightResourceIds)
        {
            if (!workItem.Subscribers.IsNullOrEmpty())
            {
                PreloadedSubscribers.UnionWith(workItem.Subscribers);
            }

            foreach (long subscriberId in ExtraSubscribersIdsToRemove)
            {
                rightResourceIds.Add(new UserAccount
                {
                    Id = subscriberId
                });
            }
        }

        private void RemoveFromTags(WorkItem workItem, ISet<IIdentifiable> rightResourceIds)
        {
            if (!workItem.Tags.IsNullOrEmpty())
            {
                PreloadedTags.UnionWith(workItem.Tags);
            }

            foreach (int tagId in ExtraTagIdsToRemove)
            {
                rightResourceIds.Add(new WorkTag
                {
                    Id = tagId
                });
            }
        }
    }
}
