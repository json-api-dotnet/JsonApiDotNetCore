using System.Net;
using DapperExample.Models;
using DapperExample.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace DapperTests.IntegrationTests.ReadWrite.Relationships;

public sealed class RemoveFromToManyRelationshipTests : IClassFixture<DapperTestContext>
{
    private readonly DapperTestContext _testContext;
    private readonly TestFakers _fakers = new();

    public RemoveFromToManyRelationshipTests(DapperTestContext testContext, ITestOutputHelper testOutputHelper)
    {
        testContext.SetTestOutputHelper(testOutputHelper);
        _testContext = testContext;
    }

    [Fact]
    public async Task Can_remove_from_OneToMany_relationship_with_nullable_foreign_key()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person existingPerson = _fakers.Person.Generate();
        existingPerson.AssignedTodoItems = _fakers.TodoItem.Generate(3).ToHashSet();
        existingPerson.AssignedTodoItems.ForEach(todoItem => todoItem.Owner = _fakers.Person.Generate());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.People.Add(existingPerson);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "todoItems",
                    id = existingPerson.AssignedTodoItems.ElementAt(0).StringId
                },
                new
                {
                    type = "todoItems",
                    id = existingPerson.AssignedTodoItems.ElementAt(2).StringId
                }
            }
        };

        string route = $"/people/{existingPerson.StringId}/relationships/assignedTodoItems";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Person personInDatabase = await dbContext.People.Include(person => person.AssignedTodoItems).FirstWithIdAsync(existingPerson.Id);

            personInDatabase.AssignedTodoItems.ShouldHaveCount(1);
            personInDatabase.AssignedTodoItems.ElementAt(0).Id.Should().Be(existingPerson.AssignedTodoItems.ElementAt(1).Id);

            List<TodoItem> todoItemInDatabases = await dbContext.TodoItems.Where(todoItem => todoItem.Assignee == null).ToListAsync();

            todoItemInDatabases.Should().HaveCount(2);
        });

        store.SqlCommands.ShouldHaveCount(3);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t3."Id"
                FROM "People" AS t1
                LEFT JOIN (
                    SELECT t2."Id", t2."AssigneeId"
                    FROM "TodoItems" AS t2
                    WHERE t2."Id" IN (@p2, @p3)
                ) AS t3 ON t1."Id" = t3."AssigneeId"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.ShouldHaveCount(3);
            command.Parameters.Should().Contain("@p1", existingPerson.Id);
            command.Parameters.Should().Contain("@p2", existingPerson.AssignedTodoItems.ElementAt(0).Id);
            command.Parameters.Should().Contain("@p3", existingPerson.AssignedTodoItems.ElementAt(2).Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id"
                FROM "TodoItems" AS t1
                WHERE t1."Id" IN (@p1, @p2)
                """));

            command.Parameters.ShouldHaveCount(2);
            command.Parameters.Should().Contain("@p1", existingPerson.AssignedTodoItems.ElementAt(0).Id);
            command.Parameters.Should().Contain("@p2", existingPerson.AssignedTodoItems.ElementAt(2).Id);
        });

        store.SqlCommands[2].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "TodoItems"
                SET "AssigneeId" = @p1
                WHERE "Id" IN (@p2, @p3)
                """));

            command.Parameters.ShouldHaveCount(3);
            command.Parameters.Should().Contain("@p1", null);
            command.Parameters.Should().Contain("@p2", existingPerson.AssignedTodoItems.ElementAt(0).Id);
            command.Parameters.Should().Contain("@p3", existingPerson.AssignedTodoItems.ElementAt(2).Id);
        });
    }

    [Fact]
    public async Task Can_remove_from_OneToMany_relationship_with_required_foreign_key()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person existingPerson = _fakers.Person.Generate();
        existingPerson.OwnedTodoItems = _fakers.TodoItem.Generate(3).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.People.Add(existingPerson);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "todoItems",
                    id = existingPerson.OwnedTodoItems.ElementAt(0).StringId
                },
                new
                {
                    type = "todoItems",
                    id = existingPerson.OwnedTodoItems.ElementAt(2).StringId
                }
            }
        };

        string route = $"/people/{existingPerson.StringId}/relationships/ownedTodoItems";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Person personInDatabase = await dbContext.People.Include(person => person.OwnedTodoItems).FirstWithIdAsync(existingPerson.Id);

            personInDatabase.OwnedTodoItems.ShouldHaveCount(1);
            personInDatabase.OwnedTodoItems.ElementAt(0).Id.Should().Be(existingPerson.OwnedTodoItems.ElementAt(1).Id);

            List<TodoItem> todoItemInDatabases = await dbContext.TodoItems.ToListAsync();

            todoItemInDatabases.Should().HaveCount(1);
        });

        store.SqlCommands.ShouldHaveCount(3);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t3."Id"
                FROM "People" AS t1
                LEFT JOIN (
                    SELECT t2."Id", t2."OwnerId"
                    FROM "TodoItems" AS t2
                    WHERE t2."Id" IN (@p2, @p3)
                ) AS t3 ON t1."Id" = t3."OwnerId"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.ShouldHaveCount(3);
            command.Parameters.Should().Contain("@p1", existingPerson.Id);
            command.Parameters.Should().Contain("@p2", existingPerson.OwnedTodoItems.ElementAt(0).Id);
            command.Parameters.Should().Contain("@p3", existingPerson.OwnedTodoItems.ElementAt(2).Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id"
                FROM "TodoItems" AS t1
                WHERE t1."Id" IN (@p1, @p2)
                """));

            command.Parameters.ShouldHaveCount(2);
            command.Parameters.Should().Contain("@p1", existingPerson.OwnedTodoItems.ElementAt(0).Id);
            command.Parameters.Should().Contain("@p2", existingPerson.OwnedTodoItems.ElementAt(2).Id);
        });

        store.SqlCommands[2].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                DELETE FROM "TodoItems"
                WHERE "Id" IN (@p1, @p2)
                """));

            command.Parameters.ShouldHaveCount(2);
            command.Parameters.Should().Contain("@p1", existingPerson.OwnedTodoItems.ElementAt(0).Id);
            command.Parameters.Should().Contain("@p2", existingPerson.OwnedTodoItems.ElementAt(2).Id);
        });
    }
}
