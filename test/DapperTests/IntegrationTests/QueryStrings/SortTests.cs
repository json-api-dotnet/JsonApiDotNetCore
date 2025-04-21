using System.Net;
using DapperExample.Models;
using DapperExample.Repositories;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace DapperTests.IntegrationTests.QueryStrings;

public sealed class SortTests : IClassFixture<DapperTestContext>
{
    private readonly DapperTestContext _testContext;
    private readonly TestFakers _fakers = new();

    public SortTests(DapperTestContext testContext, ITestOutputHelper testOutputHelper)
    {
        testContext.SetTestOutputHelper(testOutputHelper);
        _testContext = testContext;
    }

    [Fact]
    public async Task Can_sort_on_attributes_in_primary_resources()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        List<TodoItem> todoItems = _fakers.TodoItem.GenerateList(3);
        todoItems.ForEach(todoItem => todoItem.Owner = _fakers.Person.GenerateOne());

        todoItems[0].Description = "B";
        todoItems[1].Description = "A";
        todoItems[2].Description = "C";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.TodoItems.AddRange(todoItems);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/todoItems?sort=-description,durationInHours,id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);
        responseDocument.Data.ManyValue.Should().AllSatisfy(resource => resource.Type.Should().Be("todoItems"));
        responseDocument.Data.ManyValue[0].Id.Should().Be(todoItems[2].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(todoItems[0].StringId);
        responseDocument.Data.ManyValue[2].Id.Should().Be(todoItems[1].StringId);

        store.SqlCommands.Should().HaveCount(2);

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
                ORDER BY t1."Description" DESC, t1."DurationInHours", t1."Id"
                """));

            command.Parameters.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Can_sort_on_attributes_in_secondary_and_included_resources()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person person = _fakers.Person.GenerateOne();
        person.OwnedTodoItems = _fakers.TodoItem.GenerateSet(3);

        person.OwnedTodoItems.ElementAt(0).DurationInHours = 40;
        person.OwnedTodoItems.ElementAt(1).DurationInHours = 100;
        person.OwnedTodoItems.ElementAt(2).DurationInHours = 250;

        person.OwnedTodoItems.ElementAt(1).Tags = _fakers.Tag.GenerateSet(2);

        person.OwnedTodoItems.ElementAt(1).Tags.ElementAt(0).Name = "B";
        person.OwnedTodoItems.ElementAt(1).Tags.ElementAt(1).Name = "A";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.People.AddRange(person);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/people/{person.StringId}/ownedTodoItems?include=tags&sort=-durationInHours&sort[tags]=name";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);
        responseDocument.Data.ManyValue.Should().AllSatisfy(resource => resource.Type.Should().Be("todoItems"));
        responseDocument.Data.ManyValue[0].Id.Should().Be(person.OwnedTodoItems.ElementAt(2).StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(person.OwnedTodoItems.ElementAt(1).StringId);
        responseDocument.Data.ManyValue[2].Id.Should().Be(person.OwnedTodoItems.ElementAt(0).StringId);

        responseDocument.Included.Should().HaveCount(2);
        responseDocument.Included.Should().AllSatisfy(resource => resource.Type.Should().Be("tags"));
        responseDocument.Included[0].Id.Should().Be(person.OwnedTodoItems.ElementAt(1).Tags.ElementAt(1).StringId);
        responseDocument.Included[1].Id.Should().Be(person.OwnedTodoItems.ElementAt(1).Tags.ElementAt(0).StringId);

        store.SqlCommands.Should().HaveCount(2);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT COUNT(*)
                FROM "TodoItems" AS t1
                INNER JOIN "People" AS t2 ON t1."OwnerId" = t2."Id"
                WHERE t2."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", person.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t2."Id", t2."CreatedAt", t2."Description", t2."DurationInHours", t2."LastModifiedAt", t2."Priority", t3."Id", t3."Name"
                FROM "People" AS t1
                LEFT JOIN "TodoItems" AS t2 ON t1."Id" = t2."OwnerId"
                LEFT JOIN "Tags" AS t3 ON t2."Id" = t3."TodoItemId"
                WHERE t1."Id" = @p1
                ORDER BY t2."DurationInHours" DESC, t3."Name"
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", person.Id);
        });
    }

    [Fact]
    public async Task Can_sort_on_count_in_primary_resources()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        List<TodoItem> todoItems = _fakers.TodoItem.GenerateList(3);
        todoItems.ForEach(todoItem => todoItem.Owner = _fakers.Person.GenerateOne());

        todoItems[0].Tags = _fakers.Tag.GenerateSet(2);
        todoItems[1].Tags = _fakers.Tag.GenerateSet(1);
        todoItems[2].Tags = _fakers.Tag.GenerateSet(3);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.TodoItems.AddRange(todoItems);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/todoItems?sort=-count(tags),id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);
        responseDocument.Data.ManyValue.Should().AllSatisfy(resource => resource.Type.Should().Be("todoItems"));
        responseDocument.Data.ManyValue[0].Id.Should().Be(todoItems[2].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(todoItems[0].StringId);
        responseDocument.Data.ManyValue[2].Id.Should().Be(todoItems[1].StringId);

        store.SqlCommands.Should().HaveCount(2);

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
                ORDER BY (
                    SELECT COUNT(*)
                    FROM "Tags" AS t2
                    WHERE t1."Id" = t2."TodoItemId"
                ) DESC, t1."Id"
                """));

            command.Parameters.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Can_sort_on_count_in_secondary_resources()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person person = _fakers.Person.GenerateOne();
        person.OwnedTodoItems = _fakers.TodoItem.GenerateSet(3);

        person.OwnedTodoItems.ElementAt(0).Tags = _fakers.Tag.GenerateSet(2);
        person.OwnedTodoItems.ElementAt(1).Tags = _fakers.Tag.GenerateSet(1);
        person.OwnedTodoItems.ElementAt(2).Tags = _fakers.Tag.GenerateSet(3);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.People.AddRange(person);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/people/{person.StringId}/ownedTodoItems?sort=-count(tags),id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);
        responseDocument.Data.ManyValue.Should().AllSatisfy(resource => resource.Type.Should().Be("todoItems"));
        responseDocument.Data.ManyValue[0].Id.Should().Be(person.OwnedTodoItems.ElementAt(2).StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(person.OwnedTodoItems.ElementAt(0).StringId);
        responseDocument.Data.ManyValue[2].Id.Should().Be(person.OwnedTodoItems.ElementAt(1).StringId);

        store.SqlCommands.Should().HaveCount(2);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT COUNT(*)
                FROM "TodoItems" AS t1
                INNER JOIN "People" AS t2 ON t1."OwnerId" = t2."Id"
                WHERE t2."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", person.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t2."Id", t2."CreatedAt", t2."Description", t2."DurationInHours", t2."LastModifiedAt", t2."Priority"
                FROM "People" AS t1
                LEFT JOIN "TodoItems" AS t2 ON t1."Id" = t2."OwnerId"
                WHERE t1."Id" = @p1
                ORDER BY (
                    SELECT COUNT(*)
                    FROM "Tags" AS t3
                    WHERE t2."Id" = t3."TodoItemId"
                ) DESC, t2."Id"
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", person.Id);
        });
    }

    [Fact]
    public async Task Can_sort_on_count_in_secondary_resources_with_include()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person person = _fakers.Person.GenerateOne();
        person.OwnedTodoItems = _fakers.TodoItem.GenerateSet(3);

        person.OwnedTodoItems.ElementAt(0).Tags = _fakers.Tag.GenerateSet(2);
        person.OwnedTodoItems.ElementAt(1).Tags = _fakers.Tag.GenerateSet(1);
        person.OwnedTodoItems.ElementAt(2).Tags = _fakers.Tag.GenerateSet(3);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.People.AddRange(person);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/people/{person.StringId}/ownedTodoItems?include=tags&sort=-count(tags),id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);
        responseDocument.Data.ManyValue.Should().AllSatisfy(resource => resource.Type.Should().Be("todoItems"));
        responseDocument.Data.ManyValue[0].Id.Should().Be(person.OwnedTodoItems.ElementAt(2).StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(person.OwnedTodoItems.ElementAt(0).StringId);
        responseDocument.Data.ManyValue[2].Id.Should().Be(person.OwnedTodoItems.ElementAt(1).StringId);

        store.SqlCommands.Should().HaveCount(2);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT COUNT(*)
                FROM "TodoItems" AS t1
                INNER JOIN "People" AS t2 ON t1."OwnerId" = t2."Id"
                WHERE t2."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", person.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t2."Id", t2."CreatedAt", t2."Description", t2."DurationInHours", t2."LastModifiedAt", t2."Priority", t4."Id", t4."Name"
                FROM "People" AS t1
                LEFT JOIN "TodoItems" AS t2 ON t1."Id" = t2."OwnerId"
                LEFT JOIN "Tags" AS t4 ON t2."Id" = t4."TodoItemId"
                WHERE t1."Id" = @p1
                ORDER BY (
                    SELECT COUNT(*)
                    FROM "Tags" AS t3
                    WHERE t2."Id" = t3."TodoItemId"
                ) DESC, t2."Id", t4."Id"
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", person.Id);
        });
    }

    [Fact]
    public async Task Can_sort_on_count_in_included_resources()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person person = _fakers.Person.GenerateOne();
        person.OwnedTodoItems = _fakers.TodoItem.GenerateSet(4);

        person.OwnedTodoItems.ElementAt(0).Tags = _fakers.Tag.GenerateSet(2);
        person.OwnedTodoItems.ElementAt(1).Tags = _fakers.Tag.GenerateSet(1);
        person.OwnedTodoItems.ElementAt(2).Tags = _fakers.Tag.GenerateSet(3);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.People.AddRange(person);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/people?include=ownedTodoItems&sort[ownedTodoItems]=-count(tags),id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("people");
        responseDocument.Data.ManyValue[0].Id.Should().Be(person.StringId);

        responseDocument.Included.Should().HaveCount(4);
        responseDocument.Included.Should().AllSatisfy(resource => resource.Type.Should().Be("todoItems"));
        responseDocument.Included[0].Id.Should().Be(person.OwnedTodoItems.ElementAt(2).StringId);
        responseDocument.Included[1].Id.Should().Be(person.OwnedTodoItems.ElementAt(0).StringId);
        responseDocument.Included[2].Id.Should().Be(person.OwnedTodoItems.ElementAt(1).StringId);
        responseDocument.Included[3].Id.Should().Be(person.OwnedTodoItems.ElementAt(3).StringId);

        store.SqlCommands.Should().HaveCount(2);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT COUNT(*)
                FROM "People" AS t1
                """));

            command.Parameters.Should().BeEmpty();
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."FirstName", t1."LastName", t2."Id", t2."CreatedAt", t2."Description", t2."DurationInHours", t2."LastModifiedAt", t2."Priority"
                FROM "People" AS t1
                LEFT JOIN "TodoItems" AS t2 ON t1."Id" = t2."OwnerId"
                ORDER BY t1."Id", (
                    SELECT COUNT(*)
                    FROM "Tags" AS t3
                    WHERE t2."Id" = t3."TodoItemId"
                ) DESC, t2."Id"
                """));

            command.Parameters.Should().BeEmpty();
        });
    }
}
