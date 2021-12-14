using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite.Fetching;

public sealed class FetchRelationshipTests : IClassFixture<IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext> _testContext;
    private readonly ReadWriteFakers _fakers = new();

    public FetchRelationshipTests(IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<WorkItemsController>();
        testContext.UseController<UserAccountsController>();
    }

    [Fact]
    public async Task Can_get_ManyToOne_relationship()
    {
        WorkItem workItem = _fakers.WorkItem.Generate();
        workItem.Assignee = _fakers.UserAccount.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(workItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/workItems/{workItem.StringId}/relationships/assignee";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("userAccounts");
        responseDocument.Data.SingleValue.Id.Should().Be(workItem.Assignee.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().BeNull();
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();
    }

    [Fact]
    public async Task Can_get_empty_ManyToOne_relationship()
    {
        WorkItem workItem = _fakers.WorkItem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(workItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/workItems/{workItem.StringId}/relationships/assignee";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.Value.Should().BeNull();
    }

    [Fact]
    public async Task Can_get_OneToMany_relationship()
    {
        // Arrange
        UserAccount userAccount = _fakers.UserAccount.Generate();
        userAccount.AssignedItems = _fakers.WorkItem.Generate(2).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.UserAccounts.Add(userAccount);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/userAccounts/{userAccount.StringId}/relationships/assignedItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(2);

        ResourceObject item1 = responseDocument.Data.ManyValue.Single(resource => resource.Id == userAccount.AssignedItems.ElementAt(0).StringId);
        item1.Type.Should().Be("workItems");
        item1.Attributes.Should().BeNull();
        item1.Relationships.Should().BeNull();

        ResourceObject item2 = responseDocument.Data.ManyValue.Single(resource => resource.Id == userAccount.AssignedItems.ElementAt(1).StringId);
        item2.Type.Should().Be("workItems");
        item2.Attributes.Should().BeNull();
        item2.Relationships.Should().BeNull();
    }

    [Fact]
    public async Task Can_get_empty_OneToMany_relationship()
    {
        // Arrange
        UserAccount userAccount = _fakers.UserAccount.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.UserAccounts.Add(userAccount);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/userAccounts/{userAccount.StringId}/relationships/assignedItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().BeEmpty();
    }

    [Fact]
    public async Task Can_get_ManyToMany_relationship()
    {
        // Arrange
        WorkItem workItem = _fakers.WorkItem.Generate();
        workItem.Tags = _fakers.WorkTag.Generate(2).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(workItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/workItems/{workItem.StringId}/relationships/tags";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(2);

        ResourceObject item1 = responseDocument.Data.ManyValue.Single(resource => resource.Id == workItem.Tags.ElementAt(0).StringId);
        item1.Type.Should().Be("workTags");
        item1.Attributes.Should().BeNull();
        item1.Relationships.Should().BeNull();

        ResourceObject item2 = responseDocument.Data.ManyValue.Single(resource => resource.Id == workItem.Tags.ElementAt(1).StringId);
        item2.Type.Should().Be("workTags");
        item2.Attributes.Should().BeNull();
        item2.Relationships.Should().BeNull();
    }

    [Fact]
    public async Task Can_get_empty_ManyToMany_relationship()
    {
        // Arrange
        WorkItem workItem = _fakers.WorkItem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(workItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/workItems/{workItem.StringId}/relationships/tags";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().BeEmpty();
    }

    [Fact]
    public async Task Cannot_get_relationship_for_unknown_primary_type()
    {
        string route = $"/{Unknown.ResourceType}/{Unknown.StringId.Int32}/relationships/assignee";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Should().BeEmpty();
    }

    [Fact]
    public async Task Cannot_get_relationship_for_unknown_primary_ID()
    {
        string workItemId = Unknown.StringId.For<WorkItem, int>();

        string route = $"/workItems/{workItemId}/relationships/assignee";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'workItems' with ID '{workItemId}' does not exist.");
    }

    [Fact]
    public async Task Cannot_get_relationship_for_unknown_relationship_type()
    {
        WorkItem workItem = _fakers.WorkItem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(workItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/workItems/{workItem.StringId}/relationships/{Unknown.Relationship}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested relationship does not exist.");
        error.Detail.Should().Be($"Resource of type 'workItems' does not contain a relationship named '{Unknown.Relationship}'.");
    }
}
