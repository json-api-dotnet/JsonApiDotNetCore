using System.Net;
using DapperExample;
using DapperExample.Models;
using DapperExample.Repositories;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace DapperTests.IntegrationTests.ReadWrite.Resources;

public sealed class DeleteResourceTests : IClassFixture<DapperTestContext>
{
    private readonly DapperTestContext _testContext;
    private readonly TestFakers _fakers = new();

    public DeleteResourceTests(DapperTestContext testContext, ITestOutputHelper testOutputHelper)
    {
        testContext.SetTestOutputHelper(testOutputHelper);
        _testContext = testContext;
    }

    [Fact]
    public async Task Can_delete_resource_without_references()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Tag existingTag = _fakers.Tag.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.Tags.Add(existingTag);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/tags/{existingTag.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Tag? tagInDatabase = await dbContext.Tags.FirstWithIdOrDefaultAsync(existingTag.Id);
            tagInDatabase.Should().BeNull();
        });

        store.SqlCommands.Should().HaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                DELETE FROM "Tags"
                WHERE "Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingTag.Id);
        });
    }

    [Fact]
    public async Task Can_delete_resource_without_dependent_resources()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        TodoItem existingTodoItem = _fakers.TodoItem.GenerateOne();
        existingTodoItem.Owner = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.TodoItems.Add(existingTodoItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/todoItems/{existingTodoItem.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            TodoItem? todoItemInDatabase = await dbContext.TodoItems.FirstWithIdOrDefaultAsync(existingTodoItem.Id);
            todoItemInDatabase.Should().BeNull();
        });

        int index = 0;

        if (_testContext.DatabaseProvider == DatabaseProvider.SqlServer)
        {
            store.SqlCommands.Should().HaveCount(2);

            store.SqlCommands[index++].With(command =>
            {
                command.Statement.Should().Be(_testContext.AdaptSql("""
                    UPDATE "Tags"
                    SET "TodoItemId" = @p1
                    WHERE "TodoItemId" = @p2
                    """));

                command.Parameters.Should().HaveCount(2);
                command.Parameters.Should().Contain("@p1", null);
                command.Parameters.Should().Contain("@p2", existingTodoItem.Id);
            });
        }
        else
        {
            store.SqlCommands.Should().HaveCount(1);
        }

        store.SqlCommands[index].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                DELETE FROM "TodoItems"
                WHERE "Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingTodoItem.Id);
        });
    }

    [Fact]
    public async Task Can_delete_resource_with_dependent_in_OneToMany_relationship()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        TodoItem existingTodoItem = _fakers.TodoItem.GenerateOne();
        existingTodoItem.Owner = _fakers.Person.GenerateOne();
        existingTodoItem.Tags = _fakers.Tag.GenerateSet(1);
        existingTodoItem.Tags.ElementAt(0).Color = _fakers.RgbColor.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.TodoItems.Add(existingTodoItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/todoItems/{existingTodoItem.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            TodoItem? todoItemInDatabase = await dbContext.TodoItems.FirstWithIdOrDefaultAsync(existingTodoItem.Id);
            todoItemInDatabase.Should().BeNull();

            List<Tag> tags = await dbContext.Tags.Where(tag => tag.TodoItem == null).ToListAsync();
            tags.Should().HaveCount(1);
        });

        int index = 0;

        if (_testContext.DatabaseProvider == DatabaseProvider.SqlServer)
        {
            store.SqlCommands.Should().HaveCount(2);

            store.SqlCommands[index++].With(command =>
            {
                command.Statement.Should().Be(_testContext.AdaptSql("""
                    UPDATE "Tags"
                    SET "TodoItemId" = @p1
                    WHERE "TodoItemId" = @p2
                    """));

                command.Parameters.Should().HaveCount(2);
                command.Parameters.Should().Contain("@p1", null);
                command.Parameters.Should().Contain("@p2", existingTodoItem.Id);
            });
        }
        else
        {
            store.SqlCommands.Should().HaveCount(1);
        }

        store.SqlCommands[index].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                DELETE FROM "TodoItems"
                WHERE "Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingTodoItem.Id);
        });
    }

    [Fact]
    public async Task Can_delete_resource_with_dependent_in_OneToOne_relationship()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        LoginAccount existingAccount = _fakers.LoginAccount.GenerateOne();
        existingAccount.Recovery = _fakers.AccountRecovery.GenerateOne();

        Person existingPerson = _fakers.Person.GenerateOne();
        existingPerson.Account = existingAccount;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.People.Add(existingPerson);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/loginAccounts/{existingAccount.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            LoginAccount? accountInDatabase = await dbContext.LoginAccounts.FirstWithIdOrDefaultAsync(existingAccount.Id);
            accountInDatabase.Should().BeNull();

            Person personInDatabase = await dbContext.People.Include(person => person.Account).FirstWithIdAsync(existingPerson.Id);
            personInDatabase.Account.Should().BeNull();
        });

        int index = 0;

        if (_testContext.DatabaseProvider == DatabaseProvider.SqlServer)
        {
            store.SqlCommands.Should().HaveCount(2);

            store.SqlCommands[index++].With(command =>
            {
                command.Statement.Should().Be(_testContext.AdaptSql("""
                    UPDATE "People"
                    SET "AccountId" = @p1
                    WHERE "AccountId" = @p2
                    """));

                command.Parameters.Should().HaveCount(2);
                command.Parameters.Should().Contain("@p1", null);
                command.Parameters.Should().Contain("@p2", existingAccount.Id);
            });
        }
        else
        {
            store.SqlCommands.Should().HaveCount(1);
        }

        store.SqlCommands[index].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                DELETE FROM "LoginAccounts"
                WHERE "Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingAccount.Id);
        });
    }

    [Fact]
    public async Task Can_delete_resource_with_dependent_in_Cascade_relationship()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person existingPerson = _fakers.Person.GenerateOne();
        TodoItem existingTodoItem = _fakers.TodoItem.GenerateOne();
        existingTodoItem.Owner = existingPerson;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.TodoItems.Add(existingTodoItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/people/{existingPerson.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Person? personInDatabase = await dbContext.People.FirstWithIdOrDefaultAsync(existingPerson.Id);
            personInDatabase.Should().BeNull();

            TodoItem? todoItemInDatabase = await dbContext.TodoItems.FirstWithIdOrDefaultAsync(existingTodoItem.Id);
            todoItemInDatabase.Should().BeNull();
        });

        int index = 0;

        if (_testContext.DatabaseProvider == DatabaseProvider.SqlServer)
        {
            store.SqlCommands.Should().HaveCount(2);

            store.SqlCommands[index++].With(command =>
            {
                command.Statement.Should().Be(_testContext.AdaptSql("""
                    UPDATE "TodoItems"
                    SET "AssigneeId" = @p1
                    WHERE "AssigneeId" = @p2
                    """));

                command.Parameters.Should().HaveCount(2);
                command.Parameters.Should().Contain("@p1", null);
                command.Parameters.Should().Contain("@p2", existingPerson.Id);
            });
        }
        else
        {
            store.SqlCommands.Should().HaveCount(1);
        }

        store.SqlCommands[index].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                DELETE FROM "People"
                WHERE "Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingPerson.Id);
        });
    }

    [Fact]
    public async Task Cannot_delete_unknown_resource()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        const long unknownTodoItemId = Unknown.TypedId.Int64;

        string route = $"/todoItems/{unknownTodoItemId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'todoItems' with ID '{unknownTodoItemId}' does not exist.");
        error.Source.Should().BeNull();

        int index = 0;

        if (_testContext.DatabaseProvider == DatabaseProvider.SqlServer)
        {
            store.SqlCommands.Should().HaveCount(3);

            store.SqlCommands[index++].With(command =>
            {
                command.Statement.Should().Be(_testContext.AdaptSql("""
                    UPDATE "Tags"
                    SET "TodoItemId" = @p1
                    WHERE "TodoItemId" = @p2
                    """));

                command.Parameters.Should().HaveCount(2);
                command.Parameters.Should().Contain("@p1", null);
                command.Parameters.Should().Contain("@p2", unknownTodoItemId);
            });
        }
        else
        {
            store.SqlCommands.Should().HaveCount(2);
        }

        store.SqlCommands[index++].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                DELETE FROM "TodoItems"
                WHERE "Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", unknownTodoItemId);
        });

        store.SqlCommands[index].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id"
                FROM "TodoItems" AS t1
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", unknownTodoItemId);
        });
    }
}
