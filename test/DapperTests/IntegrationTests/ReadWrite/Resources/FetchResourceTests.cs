using System.Net;
using DapperExample.Models;
using DapperExample.Repositories;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace DapperTests.IntegrationTests.ReadWrite.Resources;

public sealed class FetchResourceTests : IClassFixture<DapperTestContext>
{
    private readonly DapperTestContext _testContext;
    private readonly TestFakers _fakers = new();

    public FetchResourceTests(DapperTestContext testContext, ITestOutputHelper testOutputHelper)
    {
        testContext.SetTestOutputHelper(testOutputHelper);
        _testContext = testContext;
    }

    [Fact]
    public async Task Can_get_primary_resources()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        List<TodoItem> todoItems = _fakers.TodoItem.GenerateList(2);
        todoItems.ForEach(todoItem => todoItem.Owner = _fakers.Person.GenerateOne());

        todoItems[0].Priority = TodoItemPriority.Low;
        todoItems[1].Priority = TodoItemPriority.High;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.TodoItems.AddRange(todoItems);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/todoItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue.Should().AllSatisfy(resource => resource.Type.Should().Be("todoItems"));

        responseDocument.Data.ManyValue[0].Id.Should().Be(todoItems[1].StringId);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("description").WhoseValue.Should().Be(todoItems[1].Description);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("priority").WhoseValue.Should().Be(todoItems[1].Priority);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("durationInHours").WhoseValue.Should().Be(todoItems[1].DurationInHours);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("createdAt").WhoseValue.Should().Be(todoItems[1].CreatedAt);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("modifiedAt").WhoseValue.Should().Be(todoItems[1].LastModifiedAt);
        responseDocument.Data.ManyValue[0].Relationships.Should().OnlyContainKeys("owner", "assignee", "tags");

        responseDocument.Data.ManyValue[1].Id.Should().Be(todoItems[0].StringId);
        responseDocument.Data.ManyValue[1].Attributes.Should().ContainKey("description").WhoseValue.Should().Be(todoItems[0].Description);
        responseDocument.Data.ManyValue[1].Attributes.Should().ContainKey("priority").WhoseValue.Should().Be(todoItems[0].Priority);
        responseDocument.Data.ManyValue[1].Attributes.Should().ContainKey("durationInHours").WhoseValue.Should().Be(todoItems[0].DurationInHours);
        responseDocument.Data.ManyValue[1].Attributes.Should().ContainKey("createdAt").WhoseValue.Should().Be(todoItems[0].CreatedAt);
        responseDocument.Data.ManyValue[1].Attributes.Should().ContainKey("modifiedAt").WhoseValue.Should().Be(todoItems[0].LastModifiedAt);
        responseDocument.Data.ManyValue[1].Relationships.Should().OnlyContainKeys("owner", "assignee", "tags");

        responseDocument.Meta.Should().ContainTotal(2);

        store.SqlCommands.Should().HaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."CreatedAt", t1."Description", t1."DurationInHours", t1."LastModifiedAt", t1."Priority"
                FROM "TodoItems" AS t1
                ORDER BY t1."Priority", t1."LastModifiedAt" DESC
                """));

            command.Parameters.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Can_get_primary_resource_by_ID()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        TodoItem todoItem = _fakers.TodoItem.GenerateOne();
        todoItem.Owner = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TodoItems.Add(todoItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/todoItems/{todoItem.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("todoItems");
        responseDocument.Data.SingleValue.Id.Should().Be(todoItem.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("description").WhoseValue.Should().Be(todoItem.Description);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("priority").WhoseValue.Should().Be(todoItem.Priority);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("durationInHours").WhoseValue.Should().Be(todoItem.DurationInHours);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("createdAt").WhoseValue.Should().Be(todoItem.CreatedAt);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("modifiedAt").WhoseValue.Should().Be(todoItem.LastModifiedAt);
        responseDocument.Data.SingleValue.Relationships.Should().OnlyContainKeys("owner", "assignee", "tags");

        responseDocument.Meta.Should().NotContainTotal();

        store.SqlCommands.Should().HaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."CreatedAt", t1."Description", t1."DurationInHours", t1."LastModifiedAt", t1."Priority"
                FROM "TodoItems" AS t1
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", todoItem.Id);
        });
    }

    [Fact]
    public async Task Cannot_get_unknown_primary_resource_by_ID()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        const long unknownTodoItemId = Unknown.TypedId.Int64;

        string route = $"/todoItems/{unknownTodoItemId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'todoItems' with ID '{unknownTodoItemId}' does not exist.");
        error.Source.Should().BeNull();

        store.SqlCommands.Should().HaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."CreatedAt", t1."Description", t1."DurationInHours", t1."LastModifiedAt", t1."Priority"
                FROM "TodoItems" AS t1
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", unknownTodoItemId);
        });
    }

    [Fact]
    public async Task Can_get_secondary_ToMany_resources()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        TodoItem todoItem = _fakers.TodoItem.GenerateOne();
        todoItem.Owner = _fakers.Person.GenerateOne();
        todoItem.Tags = _fakers.Tag.GenerateSet(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.TodoItems.Add(todoItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/todoItems/{todoItem.StringId}/tags";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue.Should().AllSatisfy(resource => resource.Type.Should().Be("tags"));

        responseDocument.Data.ManyValue[0].Id.Should().Be(todoItem.Tags.ElementAt(0).StringId);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("name").WhoseValue.Should().Be(todoItem.Tags.ElementAt(0).Name);
        responseDocument.Data.ManyValue[0].Relationships.Should().OnlyContainKeys("todoItem", "color");

        responseDocument.Data.ManyValue[1].Id.Should().Be(todoItem.Tags.ElementAt(1).StringId);
        responseDocument.Data.ManyValue[1].Attributes.Should().ContainKey("name").WhoseValue.Should().Be(todoItem.Tags.ElementAt(1).Name);
        responseDocument.Data.ManyValue[1].Relationships.Should().OnlyContainKeys("todoItem", "color");

        responseDocument.Meta.Should().ContainTotal(2);

        store.SqlCommands.Should().HaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t2."Id", t2."Name"
                FROM "TodoItems" AS t1
                LEFT JOIN "Tags" AS t2 ON t1."Id" = t2."TodoItemId"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", todoItem.Id);
        });
    }

    [Fact]
    public async Task Can_get_secondary_ToOne_resource()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        TodoItem todoItem = _fakers.TodoItem.GenerateOne();
        todoItem.Owner = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TodoItems.Add(todoItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/todoItems/{todoItem.StringId}/owner";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("people");
        responseDocument.Data.SingleValue.Id.Should().Be(todoItem.Owner.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("firstName").WhoseValue.Should().Be(todoItem.Owner.FirstName);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("lastName").WhoseValue.Should().Be(todoItem.Owner.LastName);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("displayName").WhoseValue.Should().Be(todoItem.Owner.DisplayName);
        responseDocument.Data.SingleValue.Relationships.Should().OnlyContainKeys("account", "ownedTodoItems", "assignedTodoItems");

        responseDocument.Meta.Should().NotContainTotal();

        store.SqlCommands.Should().HaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t2."Id", t2."FirstName", t2."LastName"
                FROM "TodoItems" AS t1
                INNER JOIN "People" AS t2 ON t1."OwnerId" = t2."Id"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", todoItem.Id);
        });
    }

    [Fact]
    public async Task Can_get_empty_secondary_ToOne_resource()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        TodoItem todoItem = _fakers.TodoItem.GenerateOne();
        todoItem.Owner = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TodoItems.Add(todoItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/todoItems/{todoItem.StringId}/assignee";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().BeNull();

        responseDocument.Meta.Should().NotContainTotal();

        store.SqlCommands.Should().HaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t2."Id", t2."FirstName", t2."LastName"
                FROM "TodoItems" AS t1
                LEFT JOIN "People" AS t2 ON t1."AssigneeId" = t2."Id"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", todoItem.Id);
        });
    }
}
