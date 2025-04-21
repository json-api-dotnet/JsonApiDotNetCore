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

public sealed class ReplaceToManyRelationshipTests : IClassFixture<DapperTestContext>
{
    private readonly DapperTestContext _testContext;
    private readonly TestFakers _fakers = new();

    public ReplaceToManyRelationshipTests(DapperTestContext testContext, ITestOutputHelper testOutputHelper)
    {
        testContext.SetTestOutputHelper(testOutputHelper);
        _testContext = testContext;
    }

    [Fact]
    public async Task Can_clear_OneToMany_relationship_with_nullable_foreign_key()
    {
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person existingPerson = _fakers.Person.GenerateOne();
        existingPerson.AssignedTodoItems = _fakers.TodoItem.GenerateSet(2);
        existingPerson.AssignedTodoItems.ForEach(todoItem => todoItem.Owner = _fakers.Person.GenerateOne());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.People.Add(existingPerson);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = Array.Empty<object>()
        };

        string route = $"/people/{existingPerson.StringId}/relationships/assignedTodoItems";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Person personInDatabase = await dbContext.People.Include(person => person.AssignedTodoItems).FirstWithIdAsync(existingPerson.Id);

            personInDatabase.AssignedTodoItems.Should().BeEmpty();
        });

        store.SqlCommands.Should().HaveCount(2);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."FirstName", t1."LastName", t2."Id", t2."CreatedAt", t2."Description", t2."DurationInHours", t2."LastModifiedAt", t2."Priority"
                FROM "People" AS t1
                LEFT JOIN "TodoItems" AS t2 ON t1."Id" = t2."AssigneeId"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingPerson.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "TodoItems"
                SET "AssigneeId" = @p1
                WHERE "Id" IN (@p2, @p3)
                """));

            command.Parameters.Should().HaveCount(3);
            command.Parameters.Should().Contain("@p1", null);
            command.Parameters.Should().Contain("@p2", existingPerson.AssignedTodoItems.ElementAt(0).Id);
            command.Parameters.Should().Contain("@p3", existingPerson.AssignedTodoItems.ElementAt(1).Id);
        });
    }

    [Fact]
    public async Task Can_clear_OneToMany_relationship_with_required_foreign_key()
    {
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person existingPerson = _fakers.Person.GenerateOne();
        existingPerson.OwnedTodoItems = _fakers.TodoItem.GenerateSet(2);
        existingPerson.OwnedTodoItems.ForEach(todoItem => todoItem.Owner = _fakers.Person.GenerateOne());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.People.Add(existingPerson);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = Array.Empty<object>()
        };

        string route = $"/people/{existingPerson.StringId}/relationships/ownedTodoItems";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Person personInDatabase = await dbContext.People.Include(person => person.OwnedTodoItems).FirstWithIdAsync(existingPerson.Id);

            personInDatabase.OwnedTodoItems.Should().BeEmpty();
        });

        store.SqlCommands.Should().HaveCount(2);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."FirstName", t1."LastName", t2."Id", t2."CreatedAt", t2."Description", t2."DurationInHours", t2."LastModifiedAt", t2."Priority"
                FROM "People" AS t1
                LEFT JOIN "TodoItems" AS t2 ON t1."Id" = t2."OwnerId"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingPerson.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                DELETE FROM "TodoItems"
                WHERE "Id" IN (@p1, @p2)
                """));

            command.Parameters.Should().HaveCount(2);
            command.Parameters.Should().Contain("@p1", existingPerson.OwnedTodoItems.ElementAt(0).Id);
            command.Parameters.Should().Contain("@p2", existingPerson.OwnedTodoItems.ElementAt(1).Id);
        });
    }

    [Fact]
    public async Task Can_create_OneToMany_relationship()
    {
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person existingPerson = _fakers.Person.GenerateOne();

        List<TodoItem> existingTodoItems = _fakers.TodoItem.GenerateList(2);
        existingTodoItems.ForEach(todoItem => todoItem.Owner = _fakers.Person.GenerateOne());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.People.Add(existingPerson);
            dbContext.TodoItems.AddRange(existingTodoItems);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "todoItems",
                    id = existingTodoItems.ElementAt(0).StringId
                },
                new
                {
                    type = "todoItems",
                    id = existingTodoItems.ElementAt(1).StringId
                }
            }
        };

        string route = $"/people/{existingPerson.StringId}/relationships/assignedTodoItems";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Person personInDatabase = await dbContext.People.Include(person => person.AssignedTodoItems).FirstWithIdAsync(existingPerson.Id);

            personInDatabase.AssignedTodoItems.Should().HaveCount(2);
            personInDatabase.AssignedTodoItems.ElementAt(0).Id.Should().Be(existingTodoItems.ElementAt(0).Id);
            personInDatabase.AssignedTodoItems.ElementAt(1).Id.Should().Be(existingTodoItems.ElementAt(1).Id);
        });

        store.SqlCommands.Should().HaveCount(2);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."FirstName", t1."LastName", t2."Id", t2."CreatedAt", t2."Description", t2."DurationInHours", t2."LastModifiedAt", t2."Priority"
                FROM "People" AS t1
                LEFT JOIN "TodoItems" AS t2 ON t1."Id" = t2."AssigneeId"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingPerson.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "TodoItems"
                SET "AssigneeId" = @p1
                WHERE "Id" IN (@p2, @p3)
                """));

            command.Parameters.Should().HaveCount(3);
            command.Parameters.Should().Contain("@p1", existingPerson.Id);
            command.Parameters.Should().Contain("@p2", existingTodoItems.ElementAt(0).Id);
            command.Parameters.Should().Contain("@p3", existingTodoItems.ElementAt(1).Id);
        });
    }

    [Fact]
    public async Task Can_replace_OneToMany_relationship_with_nullable_foreign_key()
    {
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person existingPerson = _fakers.Person.GenerateOne();
        existingPerson.AssignedTodoItems = _fakers.TodoItem.GenerateSet(1);
        existingPerson.AssignedTodoItems.ForEach(todoItem => todoItem.Owner = _fakers.Person.GenerateOne());

        TodoItem existingTodoItem = _fakers.TodoItem.GenerateOne();
        existingTodoItem.Owner = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingPerson, existingTodoItem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "todoItems",
                    id = existingTodoItem.StringId
                }
            }
        };

        string route = $"/people/{existingPerson.StringId}/relationships/assignedTodoItems";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Person personInDatabase = await dbContext.People.Include(person => person.AssignedTodoItems).FirstWithIdAsync(existingPerson.Id);

            personInDatabase.AssignedTodoItems.Should().HaveCount(1);
            personInDatabase.AssignedTodoItems.ElementAt(0).Id.Should().Be(existingTodoItem.Id);
        });

        store.SqlCommands.Should().HaveCount(3);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."FirstName", t1."LastName", t2."Id", t2."CreatedAt", t2."Description", t2."DurationInHours", t2."LastModifiedAt", t2."Priority"
                FROM "People" AS t1
                LEFT JOIN "TodoItems" AS t2 ON t1."Id" = t2."AssigneeId"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingPerson.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "TodoItems"
                SET "AssigneeId" = @p1
                WHERE "Id" = @p2
                """));

            command.Parameters.Should().HaveCount(2);
            command.Parameters.Should().Contain("@p1", null);
            command.Parameters.Should().Contain("@p2", existingPerson.AssignedTodoItems.ElementAt(0).Id);
        });

        store.SqlCommands[2].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "TodoItems"
                SET "AssigneeId" = @p1
                WHERE "Id" = @p2
                """));

            command.Parameters.Should().HaveCount(2);
            command.Parameters.Should().Contain("@p1", existingPerson.Id);
            command.Parameters.Should().Contain("@p2", existingTodoItem.Id);
        });
    }

    [Fact]
    public async Task Can_replace_OneToMany_relationship_with_required_foreign_key()
    {
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person existingPerson = _fakers.Person.GenerateOne();
        existingPerson.OwnedTodoItems = _fakers.TodoItem.GenerateSet(1);
        existingPerson.OwnedTodoItems.ForEach(todoItem => todoItem.Owner = _fakers.Person.GenerateOne());

        TodoItem existingTodoItem = _fakers.TodoItem.GenerateOne();
        existingTodoItem.Owner = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingPerson, existingTodoItem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "todoItems",
                    id = existingTodoItem.StringId
                }
            }
        };

        string route = $"/people/{existingPerson.StringId}/relationships/ownedTodoItems";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Person personInDatabase = await dbContext.People.Include(person => person.OwnedTodoItems).FirstWithIdAsync(existingPerson.Id);

            personInDatabase.OwnedTodoItems.Should().HaveCount(1);
            personInDatabase.OwnedTodoItems.ElementAt(0).Id.Should().Be(existingTodoItem.Id);
        });

        store.SqlCommands.Should().HaveCount(3);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."FirstName", t1."LastName", t2."Id", t2."CreatedAt", t2."Description", t2."DurationInHours", t2."LastModifiedAt", t2."Priority"
                FROM "People" AS t1
                LEFT JOIN "TodoItems" AS t2 ON t1."Id" = t2."OwnerId"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingPerson.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                DELETE FROM "TodoItems"
                WHERE "Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingPerson.OwnedTodoItems.ElementAt(0).Id);
        });

        store.SqlCommands[2].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "TodoItems"
                SET "OwnerId" = @p1
                WHERE "Id" = @p2
                """));

            command.Parameters.Should().HaveCount(2);
            command.Parameters.Should().Contain("@p1", existingPerson.Id);
            command.Parameters.Should().Contain("@p2", existingTodoItem.Id);
        });
    }
}
