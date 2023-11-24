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

        List<TodoItem> todoItems = _fakers.TodoItem.Generate(2);
        todoItems.ForEach(todoItem => todoItem.Owner = _fakers.Person.Generate());

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

        responseDocument.Data.ManyValue.ShouldHaveCount(2);
        responseDocument.Data.ManyValue.Should().AllSatisfy(resource => resource.Type.Should().Be("todoItems"));

        responseDocument.Data.ManyValue[0].Id.Should().Be(todoItems[1].StringId);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("description").With(value => value.Should().Be(todoItems[1].Description));
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("priority").With(value => value.Should().Be(todoItems[1].Priority));
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("durationInHours").With(value => value.Should().Be(todoItems[1].DurationInHours));
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("createdAt").With(value => value.Should().Be(todoItems[1].CreatedAt));
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("modifiedAt").With(value => value.Should().Be(todoItems[1].LastModifiedAt));
        responseDocument.Data.ManyValue[0].Relationships.ShouldOnlyContainKeys("owner", "assignee", "tags");

        responseDocument.Data.ManyValue[1].Id.Should().Be(todoItems[0].StringId);
        responseDocument.Data.ManyValue[1].Attributes.ShouldContainKey("description").With(value => value.Should().Be(todoItems[0].Description));
        responseDocument.Data.ManyValue[1].Attributes.ShouldContainKey("priority").With(value => value.Should().Be(todoItems[0].Priority));
        responseDocument.Data.ManyValue[1].Attributes.ShouldContainKey("durationInHours").With(value => value.Should().Be(todoItems[0].DurationInHours));
        responseDocument.Data.ManyValue[1].Attributes.ShouldContainKey("createdAt").With(value => value.Should().Be(todoItems[0].CreatedAt));
        responseDocument.Data.ManyValue[1].Attributes.ShouldContainKey("modifiedAt").With(value => value.Should().Be(todoItems[0].LastModifiedAt));
        responseDocument.Data.ManyValue[1].Relationships.ShouldOnlyContainKeys("owner", "assignee", "tags");

        responseDocument.Meta.Should().ContainTotal(2);

        store.SqlCommands.ShouldHaveCount(2);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT COUNT(*)
                FROM "TodoItems" AS t1
                """));

            command.Parameters.Should().BeEmpty();
        });

        store.SqlCommands[1].With(command =>
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

        TodoItem todoItem = _fakers.TodoItem.Generate();
        todoItem.Owner = _fakers.Person.Generate();

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

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("todoItems");
        responseDocument.Data.SingleValue.Id.Should().Be(todoItem.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("description").With(value => value.Should().Be(todoItem.Description));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("priority").With(value => value.Should().Be(todoItem.Priority));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("durationInHours").With(value => value.Should().Be(todoItem.DurationInHours));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("createdAt").With(value => value.Should().Be(todoItem.CreatedAt));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("modifiedAt").With(value => value.Should().Be(todoItem.LastModifiedAt));
        responseDocument.Data.SingleValue.Relationships.ShouldOnlyContainKeys("owner", "assignee", "tags");

        responseDocument.Meta.Should().BeNull();

        store.SqlCommands.ShouldHaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."CreatedAt", t1."Description", t1."DurationInHours", t1."LastModifiedAt", t1."Priority"
                FROM "TodoItems" AS t1
                WHERE t1."Id" = @p1
                """));

            command.Parameters.ShouldHaveCount(1);
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

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'todoItems' with ID '{unknownTodoItemId}' does not exist.");
        error.Source.Should().BeNull();

        store.SqlCommands.ShouldHaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."CreatedAt", t1."Description", t1."DurationInHours", t1."LastModifiedAt", t1."Priority"
                FROM "TodoItems" AS t1
                WHERE t1."Id" = @p1
                """));

            command.Parameters.ShouldHaveCount(1);
            command.Parameters.Should().Contain("@p1", unknownTodoItemId);
        });
    }

    [Fact]
    public async Task Can_get_secondary_ToMany_resources()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        TodoItem todoItem = _fakers.TodoItem.Generate();
        todoItem.Owner = _fakers.Person.Generate();
        todoItem.Tags = _fakers.Tag.Generate(2).ToHashSet();

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

        responseDocument.Data.ManyValue.ShouldHaveCount(2);
        responseDocument.Data.ManyValue.Should().AllSatisfy(resource => resource.Type.Should().Be("tags"));

        responseDocument.Data.ManyValue[0].Id.Should().Be(todoItem.Tags.ElementAt(0).StringId);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("name").With(value => value.Should().Be(todoItem.Tags.ElementAt(0).Name));
        responseDocument.Data.ManyValue[0].Relationships.ShouldOnlyContainKeys("todoItem", "color");

        responseDocument.Data.ManyValue[1].Id.Should().Be(todoItem.Tags.ElementAt(1).StringId);
        responseDocument.Data.ManyValue[1].Attributes.ShouldContainKey("name").With(value => value.Should().Be(todoItem.Tags.ElementAt(1).Name));
        responseDocument.Data.ManyValue[1].Relationships.ShouldOnlyContainKeys("todoItem", "color");

        responseDocument.Meta.Should().ContainTotal(2);

        store.SqlCommands.ShouldHaveCount(2);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT COUNT(*)
                FROM "Tags" AS t1
                LEFT JOIN "TodoItems" AS t2 ON t1."TodoItemId" = t2."Id"
                WHERE t2."Id" = @p1
                """));

            command.Parameters.ShouldHaveCount(1);
            command.Parameters.Should().Contain("@p1", todoItem.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t2."Id", t2."Name"
                FROM "TodoItems" AS t1
                LEFT JOIN "Tags" AS t2 ON t1."Id" = t2."TodoItemId"
                WHERE t1."Id" = @p1
                ORDER BY t2."Id"
                """));

            command.Parameters.ShouldHaveCount(1);
            command.Parameters.Should().Contain("@p1", todoItem.Id);
        });
    }

    [Fact]
    public async Task Can_get_secondary_ToOne_resource()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        TodoItem todoItem = _fakers.TodoItem.Generate();
        todoItem.Owner = _fakers.Person.Generate();

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

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("people");
        responseDocument.Data.SingleValue.Id.Should().Be(todoItem.Owner.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("firstName").With(value => value.Should().Be(todoItem.Owner.FirstName));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("lastName").With(value => value.Should().Be(todoItem.Owner.LastName));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("displayName").With(value => value.Should().Be(todoItem.Owner.DisplayName));
        responseDocument.Data.SingleValue.Relationships.ShouldOnlyContainKeys("account", "ownedTodoItems", "assignedTodoItems");

        responseDocument.Meta.Should().BeNull();

        store.SqlCommands.ShouldHaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t2."Id", t2."FirstName", t2."LastName"
                FROM "TodoItems" AS t1
                INNER JOIN "People" AS t2 ON t1."OwnerId" = t2."Id"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.ShouldHaveCount(1);
            command.Parameters.Should().Contain("@p1", todoItem.Id);
        });
    }

    [Fact]
    public async Task Can_get_empty_secondary_ToOne_resource()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        TodoItem todoItem = _fakers.TodoItem.Generate();
        todoItem.Owner = _fakers.Person.Generate();

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

        responseDocument.Meta.Should().BeNull();

        store.SqlCommands.ShouldHaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t2."Id", t2."FirstName", t2."LastName"
                FROM "TodoItems" AS t1
                LEFT JOIN "People" AS t2 ON t1."AssigneeId" = t2."Id"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.ShouldHaveCount(1);
            command.Parameters.Should().Contain("@p1", todoItem.Id);
        });
    }
}
