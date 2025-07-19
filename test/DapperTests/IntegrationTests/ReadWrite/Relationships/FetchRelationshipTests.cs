using System.Net;
using DapperExample.Models;
using DapperExample.Repositories;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace DapperTests.IntegrationTests.ReadWrite.Relationships;

public sealed class FetchRelationshipTests : IClassFixture<DapperTestContext>
{
    private readonly DapperTestContext _testContext;
    private readonly TestFakers _fakers = new();

    public FetchRelationshipTests(DapperTestContext testContext, ITestOutputHelper testOutputHelper)
    {
        testContext.SetTestOutputHelper(testOutputHelper);
        _testContext = testContext;
    }

    [Fact]
    public async Task Can_get_ToOne_relationship()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        TodoItem todoItem = _fakers.TodoItem.GenerateOne();
        todoItem.Owner = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.TodoItems.Add(todoItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/todoItems/{todoItem.StringId}/relationships/owner";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("people");
        responseDocument.Data.SingleValue.Id.Should().Be(todoItem.Owner.StringId);

        responseDocument.Meta.Should().BeNull();

        store.SqlCommands.Should().HaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t2."Id"
                FROM "TodoItems" AS t1
                INNER JOIN "People" AS t2 ON t1."OwnerId" = t2."Id"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", todoItem.Id);
        });
    }

    [Fact]
    public async Task Can_get_empty_ToOne_relationship()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        TodoItem todoItem = _fakers.TodoItem.GenerateOne();
        todoItem.Owner = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.TodoItems.Add(todoItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/todoItems/{todoItem.StringId}/relationships/assignee";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.Value.Should().BeNull();

        responseDocument.Meta.Should().BeNull();

        store.SqlCommands.Should().HaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t2."Id"
                FROM "TodoItems" AS t1
                LEFT JOIN "People" AS t2 ON t1."AssigneeId" = t2."Id"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", todoItem.Id);
        });
    }

    [Fact]
    public async Task Can_get_ToMany_relationship()
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

        string route = $"/todoItems/{todoItem.StringId}/relationships/tags";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue.Should().AllSatisfy(resource => resource.Type.Should().Be("tags"));
        responseDocument.Data.ManyValue[0].Id.Should().Be(todoItem.Tags.ElementAt(0).StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(todoItem.Tags.ElementAt(1).StringId);

        responseDocument.Meta.Should().ContainTotal(2);

        store.SqlCommands.Should().HaveCount(2);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT COUNT(*)
                FROM "Tags" AS t1
                LEFT JOIN "TodoItems" AS t2 ON t1."TodoItemId" = t2."Id"
                WHERE t2."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", todoItem.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t2."Id"
                FROM "TodoItems" AS t1
                LEFT JOIN "Tags" AS t2 ON t1."Id" = t2."TodoItemId"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", todoItem.Id);
        });
    }

    [Fact]
    public async Task Cannot_get_relationship_for_unknown_primary_ID()
    {
        const long unknownTodoItemId = Unknown.TypedId.Int64;

        string route = $"/todoItems/{unknownTodoItemId}/relationships/owner";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'todoItems' with ID '{unknownTodoItemId}' does not exist.");
    }
}
