using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite.Updating.Relationships;

public sealed class UpdateToOneRelationshipTests : IClassFixture<IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext> _testContext;
    private readonly ReadWriteFakers _fakers = new();

    public UpdateToOneRelationshipTests(IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<WorkItemsController>();
        testContext.UseController<WorkItemGroupsController>();
        testContext.UseController<RgbColorsController>();
    }

    [Fact]
    public async Task Can_clear_ManyToOne_relationship()
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
            data = (object?)null
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/assignee";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Assignee).FirstWithIdAsync(existingWorkItem.Id);

            workItemInDatabase.Assignee.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_clear_OneToOne_relationship()
    {
        // Arrange
        WorkItemGroup existingGroup = _fakers.WorkItemGroup.GenerateOne();
        existingGroup.Color = _fakers.RgbColor.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Groups.Add(existingGroup);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = (object?)null
        };

        string route = $"/workItemGroups/{existingGroup.StringId}/relationships/color";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItemGroup groupInDatabase = await dbContext.Groups.Include(group => group.Color).FirstWithIdAsync(existingGroup.Id);

            groupInDatabase.Color.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_create_OneToOne_relationship_from_dependent_side()
    {
        // Arrange
        WorkItemGroup existingGroup = _fakers.WorkItemGroup.GenerateOne();
        existingGroup.Color = _fakers.RgbColor.GenerateOne();

        RgbColor existingColor = _fakers.RgbColor.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingGroup, existingColor);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "workItemGroups",
                id = existingGroup.StringId
            }
        };

        string route = $"/rgbColors/{existingColor.StringId}/relationships/group";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<RgbColor> colorsInDatabase = await dbContext.RgbColors.Include(rgbColor => rgbColor.Group).ToListAsync();

            RgbColor colorInDatabase1 = colorsInDatabase.Single(color => color.Id == existingGroup.Color.Id);
            colorInDatabase1.Group.Should().BeNull();

            RgbColor colorInDatabase2 = colorsInDatabase.Single(color => color.Id == existingColor.Id);
            colorInDatabase2.Group.Should().NotBeNull();
            colorInDatabase2.Group.Id.Should().Be(existingGroup.Id);
        });
    }

    [Fact]
    public async Task Can_replace_OneToOne_relationship_from_principal_side()
    {
        // Arrange
        List<WorkItemGroup> existingGroups = _fakers.WorkItemGroup.GenerateList(2);
        existingGroups[0].Color = _fakers.RgbColor.GenerateOne();
        existingGroups[1].Color = _fakers.RgbColor.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Groups.AddRange(existingGroups);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "rgbColors",
                id = existingGroups[0].Color!.StringId
            }
        };

        string route = $"/workItemGroups/{existingGroups[1].StringId}/relationships/color";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<WorkItemGroup> groupsInDatabase = await dbContext.Groups.Include(group => group.Color).ToListAsync();

            WorkItemGroup groupInDatabase1 = groupsInDatabase.Single(group => group.Id == existingGroups[0].Id);
            groupInDatabase1.Color.Should().BeNull();

            WorkItemGroup groupInDatabase2 = groupsInDatabase.Single(group => group.Id == existingGroups[1].Id);
            groupInDatabase2.Color.Should().NotBeNull();
            groupInDatabase2.Color.Id.Should().Be(existingGroups[0].Color!.Id);

            List<RgbColor> colorsInDatabase = await dbContext.RgbColors.Include(color => color.Group).ToListAsync();

            RgbColor colorInDatabase1 = colorsInDatabase.Single(color => color.Id == existingGroups[0].Color!.Id);
            colorInDatabase1.Group.Should().NotBeNull();
            colorInDatabase1.Group.Id.Should().Be(existingGroups[1].Id);

            RgbColor? colorInDatabase2 = colorsInDatabase.SingleOrDefault(color => color.Id == existingGroups[1].Color!.Id);
            colorInDatabase2.Should().NotBeNull();
            colorInDatabase2.Group.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_replace_ManyToOne_relationship()
    {
        // Arrange
        List<UserAccount> existingUserAccounts = _fakers.UserAccount.GenerateList(2);
        existingUserAccounts[0].AssignedItems = _fakers.WorkItem.GenerateSet(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.UserAccounts.AddRange(existingUserAccounts);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "userAccounts",
                id = existingUserAccounts[1].StringId
            }
        };

        string route = $"/workItems/{existingUserAccounts[0].AssignedItems.ElementAt(1).StringId}/relationships/assignee";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            int workItemId = existingUserAccounts[0].AssignedItems.ElementAt(1).Id;

            WorkItem workItemInDatabase2 = await dbContext.WorkItems.Include(workItem => workItem.Assignee).FirstWithIdAsync(workItemId);

            workItemInDatabase2.Assignee.Should().NotBeNull();
            workItemInDatabase2.Assignee.Id.Should().Be(existingUserAccounts[1].Id);
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

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/assignee";

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

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/assignee";

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
    public async Task Cannot_create_for_missing_data()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();
        UserAccount existingUserAccount = _fakers.UserAccount.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingWorkItem, existingUserAccount);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/assignee";

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
    public async Task Cannot_create_for_array_data()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();
        UserAccount existingUserAccount = _fakers.UserAccount.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingWorkItem, existingUserAccount);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "userAccounts",
                    id = existingUserAccount.StringId
                }
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/assignee";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Expected an object or 'null', instead of an array.");
        error.Detail.Should().BeNull();
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_create_for_missing_type()
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
                id = Unknown.StringId.For<UserAccount, long>()
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/assignee";

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
        error.Source.Pointer.Should().Be("/data");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_create_for_unknown_type()
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
                type = Unknown.ResourceType,
                id = Unknown.StringId.For<UserAccount, long>()
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/assignee";

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
        error.Source.Pointer.Should().Be("/data/type");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_create_for_missing_ID()
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
                type = "userAccounts"
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/assignee";

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
        error.Source.Pointer.Should().Be("/data");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_create_with_unknown_ID()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        string userAccountId = Unknown.StringId.For<UserAccount, long>();

        var requestBody = new
        {
            data = new
            {
                type = "userAccounts",
                id = userAccountId
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/assignee";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'userAccounts' with ID '{userAccountId}' in relationship 'assignee' does not exist.");
        error.Source.Should().BeNull();
        error.Meta.Should().NotContainKey("requestBody");
    }

    [Fact]
    public async Task Cannot_create_on_unknown_resource_type_in_url()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();
        UserAccount existingUserAccount = _fakers.UserAccount.GenerateOne();

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

        string route = $"/{Unknown.ResourceType}/{existingWorkItem.StringId}/relationships/assignee";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Should().BeEmpty();
    }

    [Fact]
    public async Task Cannot_create_on_unknown_resource_ID_in_url()
    {
        // Arrange
        UserAccount existingUserAccount = _fakers.UserAccount.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.UserAccounts.Add(existingUserAccount);
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

        string workItemId = Unknown.StringId.For<WorkItem, int>();

        string route = $"/workItems/{workItemId}/relationships/assignee";

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
    public async Task Cannot_create_on_unknown_relationship_in_url()
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
                type = "userAccounts",
                id = Unknown.StringId.For<UserAccount, long>()
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
    public async Task Cannot_create_on_whitespace_relationship_in_url()
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
                type = "userAccounts",
                id = Unknown.StringId.For<UserAccount, long>()
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/%20";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

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
    public async Task Cannot_create_on_relationship_mismatch_between_url_and_body()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();
        RgbColor existingColor = _fakers.RgbColor.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingWorkItem, existingColor);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "rgbColors",
                id = existingColor.StringId
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/assignee";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().Be("Failed to deserialize request body: Incompatible resource type found.");
        error.Detail.Should().Be("Type 'rgbColors' is not convertible to type 'userAccounts' of relationship 'assignee'.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/type");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Can_clear_cyclic_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();

            existingWorkItem.Parent = existingWorkItem;
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = (object?)null
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/parent";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Parent).FirstWithIdAsync(existingWorkItem.Id);

            workItemInDatabase.Parent.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_assign_cyclic_relationship()
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
                type = "workItems",
                id = existingWorkItem.StringId
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/parent";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        // Assert
        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Parent).FirstWithIdAsync(existingWorkItem.Id);

            workItemInDatabase.Parent.Should().NotBeNull();
            workItemInDatabase.Parent.Id.Should().Be(existingWorkItem.Id);
        });
    }

    [Fact]
    public async Task Cannot_assign_relationship_with_blocked_capability()
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
                type = "workItemGroups",
                id = Unknown.StringId.For<WorkItemGroup, Guid>()
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/group";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Relationship cannot be assigned.");
        error.Detail.Should().Be("The relationship 'group' on resource type 'workItems' cannot be assigned to.");
        error.Source.Should().BeNull();
        error.Meta.Should().HaveRequestBody();
    }
}
