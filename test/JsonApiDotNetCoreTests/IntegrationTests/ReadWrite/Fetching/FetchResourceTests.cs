using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite.Fetching;

public sealed class FetchResourceTests : IClassFixture<IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext> _testContext;
    private readonly ReadWriteFakers _fakers = new();

    public FetchResourceTests(IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<WorkItemsController>();
        testContext.UseController<UserAccountsController>();
        testContext.UseController<WorkTagsController>();
    }

    [Fact]
    public async Task Can_get_primary_resources()
    {
        // Arrange
        List<WorkItem> workItems = _fakers.WorkItem.Generate(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<WorkItem>();
            dbContext.WorkItems.AddRange(workItems);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/workItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(2);

        ResourceObject item1 = responseDocument.Data.ManyValue.Single(resource => resource.Id == workItems[0].StringId);
        item1.Type.Should().Be("workItems");
        item1.Attributes.ShouldContainKey("description").With(value => value.Should().Be(workItems[0].Description));
        item1.Attributes.ShouldContainKey("dueAt").With(value => value.Should().Be(workItems[0].DueAt));
        item1.Attributes.ShouldContainKey("priority").With(value => value.Should().Be(workItems[0].Priority));
        item1.Relationships.ShouldNotBeEmpty();

        ResourceObject item2 = responseDocument.Data.ManyValue.Single(resource => resource.Id == workItems[1].StringId);
        item2.Type.Should().Be("workItems");
        item2.Attributes.ShouldContainKey("description").With(value => value.Should().Be(workItems[1].Description));
        item2.Attributes.ShouldContainKey("dueAt").With(value => value.Should().Be(workItems[1].DueAt));
        item2.Attributes.ShouldContainKey("priority").With(value => value.Should().Be(workItems[1].Priority));
        item2.Relationships.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Cannot_get_primary_resources_for_unknown_type()
    {
        // Arrange
        const string route = "/" + Unknown.ResourceType;

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Should().BeEmpty();
    }

    [Fact]
    public async Task Can_get_primary_resource_by_ID()
    {
        // Arrange
        WorkItem workItem = _fakers.WorkItem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(workItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/workItems/{workItem.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("workItems");
        responseDocument.Data.SingleValue.Id.Should().Be(workItem.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("description").With(value => value.Should().Be(workItem.Description));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("dueAt").With(value => value.Should().Be(workItem.DueAt));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("priority").With(value => value.Should().Be(workItem.Priority));
        responseDocument.Data.SingleValue.Relationships.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Cannot_get_primary_resource_for_unknown_type()
    {
        // Arrange
        string route = $"/{Unknown.ResourceType}/{Unknown.StringId.Int32}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Should().BeEmpty();
    }

    [Fact]
    public async Task Cannot_get_primary_resource_for_unknown_ID()
    {
        // Arrange
        string workItemId = Unknown.StringId.For<WorkItem, int>();

        string route = $"/workItems/{workItemId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'workItems' with ID '{workItemId}' does not exist.");
    }

    [Fact]
    public async Task Can_get_secondary_ManyToOne_resource()
    {
        // Arrange
        WorkItem workItem = _fakers.WorkItem.Generate();
        workItem.Assignee = _fakers.UserAccount.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(workItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/workItems/{workItem.StringId}/assignee";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("userAccounts");
        responseDocument.Data.SingleValue.Id.Should().Be(workItem.Assignee.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("firstName").With(value => value.Should().Be(workItem.Assignee.FirstName));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("lastName").With(value => value.Should().Be(workItem.Assignee.LastName));
        responseDocument.Data.SingleValue.Relationships.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Can_get_unknown_secondary_ManyToOne_resource()
    {
        // Arrange
        WorkItem workItem = _fakers.WorkItem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(workItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/workItems/{workItem.StringId}/assignee";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.Value.Should().BeNull();
    }

    [Fact]
    public async Task Can_get_secondary_OneToMany_resources()
    {
        // Arrange
        UserAccount userAccount = _fakers.UserAccount.Generate();
        userAccount.AssignedItems = _fakers.WorkItem.Generate(2).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.UserAccounts.Add(userAccount);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/userAccounts/{userAccount.StringId}/assignedItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(2);

        ResourceObject item1 = responseDocument.Data.ManyValue.Single(resource => resource.Id == userAccount.AssignedItems.ElementAt(0).StringId);
        item1.Type.Should().Be("workItems");
        item1.Attributes.ShouldContainKey("description").With(value => value.Should().Be(userAccount.AssignedItems.ElementAt(0).Description));
        item1.Attributes.ShouldContainKey("dueAt").With(value => value.Should().Be(userAccount.AssignedItems.ElementAt(0).DueAt));
        item1.Attributes.ShouldContainKey("priority").With(value => value.Should().Be(userAccount.AssignedItems.ElementAt(0).Priority));
        item1.Relationships.ShouldNotBeEmpty();

        ResourceObject item2 = responseDocument.Data.ManyValue.Single(resource => resource.Id == userAccount.AssignedItems.ElementAt(1).StringId);
        item2.Type.Should().Be("workItems");
        item2.Attributes.ShouldContainKey("description").With(value => value.Should().Be(userAccount.AssignedItems.ElementAt(1).Description));
        item2.Attributes.ShouldContainKey("dueAt").With(value => value.Should().Be(userAccount.AssignedItems.ElementAt(1).DueAt));
        item2.Attributes.ShouldContainKey("priority").With(value => value.Should().Be(userAccount.AssignedItems.ElementAt(1).Priority));
        item2.Relationships.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Can_get_unknown_secondary_OneToMany_resource()
    {
        // Arrange
        UserAccount userAccount = _fakers.UserAccount.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.UserAccounts.Add(userAccount);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/userAccounts/{userAccount.StringId}/assignedItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().BeEmpty();
    }

    [Fact]
    public async Task Can_get_secondary_ManyToMany_resources()
    {
        // Arrange
        WorkItem workItem = _fakers.WorkItem.Generate();
        workItem.Tags = _fakers.WorkTag.Generate(2).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(workItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/workItems/{workItem.StringId}/tags";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(2);

        ResourceObject item1 = responseDocument.Data.ManyValue.Single(resource => resource.Id == workItem.Tags.ElementAt(0).StringId);
        item1.Type.Should().Be("workTags");
        item1.Attributes.ShouldContainKey("text").With(value => value.Should().Be(workItem.Tags.ElementAt(0).Text));
        item1.Attributes.ShouldContainKey("isBuiltIn").With(value => value.Should().Be(workItem.Tags.ElementAt(0).IsBuiltIn));
        item1.Relationships.ShouldNotBeEmpty();

        ResourceObject item2 = responseDocument.Data.ManyValue.Single(resource => resource.Id == workItem.Tags.ElementAt(1).StringId);
        item2.Type.Should().Be("workTags");
        item2.Attributes.ShouldContainKey("text").With(value => value.Should().Be(workItem.Tags.ElementAt(1).Text));
        item2.Attributes.ShouldContainKey("isBuiltIn").With(value => value.Should().Be(workItem.Tags.ElementAt(1).IsBuiltIn));
        item2.Relationships.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Can_get_unknown_secondary_ManyToMany_resources()
    {
        // Arrange
        WorkItem workItem = _fakers.WorkItem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(workItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/workItems/{workItem.StringId}/tags";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().BeEmpty();
    }

    [Fact]
    public async Task Cannot_get_secondary_resource_for_unknown_primary_type()
    {
        // Arrange
        string route = $"/{Unknown.ResourceType}/{Unknown.StringId.Int32}/assignee";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Should().BeEmpty();
    }

    [Fact]
    public async Task Cannot_get_secondary_resource_for_unknown_primary_ID()
    {
        // Arrange
        string workItemId = Unknown.StringId.For<WorkItem, int>();

        string route = $"/workItems/{workItemId}/assignee";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'workItems' with ID '{workItemId}' does not exist.");
    }

    [Fact]
    public async Task Cannot_get_secondary_resource_for_unknown_secondary_type()
    {
        // Arrange
        WorkItem workItem = _fakers.WorkItem.Generate();
        workItem.Assignee = _fakers.UserAccount.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(workItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/workItems/{workItem.StringId}/{Unknown.Relationship}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested relationship does not exist.");
        error.Detail.Should().Be($"Resource of type 'workItems' does not contain a relationship named '{Unknown.Relationship}'.");
    }
}
