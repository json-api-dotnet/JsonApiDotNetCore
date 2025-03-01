using System.Net;
using DapperExample.Models;
using DapperExample.Repositories;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace DapperTests.IntegrationTests.ReadWrite.Relationships;

public sealed class UpdateToOneRelationshipTests : IClassFixture<DapperTestContext>
{
    private readonly DapperTestContext _testContext;
    private readonly TestFakers _fakers = new();

    public UpdateToOneRelationshipTests(DapperTestContext testContext, ITestOutputHelper testOutputHelper)
    {
        testContext.SetTestOutputHelper(testOutputHelper);
        _testContext = testContext;
    }

    [Fact]
    public async Task Can_clear_OneToOne_relationship_with_nullable_foreign_key_at_left_side()
    {
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person existingPerson = _fakers.Person.GenerateOne();
        existingPerson.Account = _fakers.LoginAccount.GenerateOne();
        existingPerson.Account.Recovery = _fakers.AccountRecovery.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.People.Add(existingPerson);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = (object?)null
        };

        string route = $"/people/{existingPerson.StringId}/relationships/account";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Person personInDatabase = await dbContext.People.Include(person => person.Account).FirstWithIdAsync(existingPerson.Id);

            personInDatabase.Account.Should().BeNull();

            LoginAccount loginAccountInDatabase =
                await dbContext.LoginAccounts.Include(loginAccount => loginAccount.Person).FirstWithIdAsync(existingPerson.Account.Id);

            loginAccountInDatabase.Person.Should().BeNull();
        });

        store.SqlCommands.Should().HaveCount(2);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."FirstName", t1."LastName", t2."Id", t2."LastUsedAt", t2."UserName"
                FROM "People" AS t1
                LEFT JOIN "LoginAccounts" AS t2 ON t1."AccountId" = t2."Id"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingPerson.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "People"
                SET "AccountId" = @p1
                WHERE "Id" = @p2
                """));

            command.Parameters.Should().HaveCount(2);
            command.Parameters.Should().Contain("@p1", null);
            command.Parameters.Should().Contain("@p2", existingPerson.Id);
        });
    }

    [Fact]
    public async Task Can_clear_OneToOne_relationship_with_nullable_foreign_key_at_right_side()
    {
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        LoginAccount existingLoginAccount = _fakers.LoginAccount.GenerateOne();
        existingLoginAccount.Recovery = _fakers.AccountRecovery.GenerateOne();
        existingLoginAccount.Person = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.LoginAccounts.Add(existingLoginAccount);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = (object?)null
        };

        string route = $"/loginAccounts/{existingLoginAccount.StringId}/relationships/person";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            LoginAccount loginAccountInDatabase =
                await dbContext.LoginAccounts.Include(loginAccount => loginAccount.Person).FirstWithIdAsync(existingLoginAccount.Id);

            loginAccountInDatabase.Person.Should().BeNull();

            Person personInDatabase = await dbContext.People.Include(person => person.Account).FirstWithIdAsync(existingLoginAccount.Person.Id);

            personInDatabase.Account.Should().BeNull();
        });

        store.SqlCommands.Should().HaveCount(2);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."LastUsedAt", t1."UserName", t2."Id", t2."FirstName", t2."LastName"
                FROM "LoginAccounts" AS t1
                LEFT JOIN "People" AS t2 ON t1."Id" = t2."AccountId"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingLoginAccount.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "People"
                SET "AccountId" = @p1
                WHERE "Id" = @p2
                """));

            command.Parameters.Should().HaveCount(2);
            command.Parameters.Should().Contain("@p1", null);
            command.Parameters.Should().Contain("@p2", existingLoginAccount.Person.Id);
        });
    }

    [Fact]
    public async Task Can_clear_OneToOne_relationship_with_nullable_foreign_key_at_right_side_when_already_null()
    {
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        LoginAccount existingLoginAccount = _fakers.LoginAccount.GenerateOne();
        existingLoginAccount.Recovery = _fakers.AccountRecovery.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.LoginAccounts.Add(existingLoginAccount);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = (object?)null
        };

        string route = $"/loginAccounts/{existingLoginAccount.StringId}/relationships/person";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        store.SqlCommands.Should().HaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."LastUsedAt", t1."UserName", t2."Id", t2."FirstName", t2."LastName"
                FROM "LoginAccounts" AS t1
                LEFT JOIN "People" AS t2 ON t1."Id" = t2."AccountId"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingLoginAccount.Id);
        });
    }

    [Fact]
    public async Task Cannot_clear_OneToOne_relationship_with_required_foreign_key_at_left_side()
    {
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        LoginAccount existingLoginAccount = _fakers.LoginAccount.GenerateOne();
        existingLoginAccount.Recovery = _fakers.AccountRecovery.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.LoginAccounts.Add(existingLoginAccount);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = (object?)null
        };

        string route = $"/loginAccounts/{existingLoginAccount.StringId}/relationships/recovery";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Failed to clear a required relationship.");
        error.Detail.Should().Be("The relationship 'recovery' on resource type 'loginAccounts' cannot be cleared because it is a required relationship.");
        error.Source.Should().BeNull();

        store.SqlCommands.Should().HaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."LastUsedAt", t1."UserName", t2."Id", t2."EmailAddress", t2."PhoneNumber"
                FROM "LoginAccounts" AS t1
                INNER JOIN "AccountRecoveries" AS t2 ON t1."RecoveryId" = t2."Id"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingLoginAccount.Id);
        });
    }

    [Fact]
    public async Task Cannot_clear_OneToOne_relationship_with_required_foreign_key_at_right_side()
    {
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        AccountRecovery existingAccountRecovery = _fakers.AccountRecovery.GenerateOne();
        existingAccountRecovery.Account = _fakers.LoginAccount.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AccountRecoveries.Add(existingAccountRecovery);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = (object?)null
        };

        string route = $"/accountRecoveries/{existingAccountRecovery.StringId}/relationships/account";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Failed to clear a required relationship.");
        error.Detail.Should().Be("The relationship 'account' on resource type 'accountRecoveries' cannot be cleared because it is a required relationship.");
        error.Source.Should().BeNull();

        store.SqlCommands.Should().HaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."EmailAddress", t1."PhoneNumber", t2."Id", t2."LastUsedAt", t2."UserName"
                FROM "AccountRecoveries" AS t1
                LEFT JOIN "LoginAccounts" AS t2 ON t1."Id" = t2."RecoveryId"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingAccountRecovery.Id);
        });
    }

    [Fact]
    public async Task Can_clear_ManyToOne_relationship_with_nullable_foreign_key()
    {
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        TodoItem existingTodoItem = _fakers.TodoItem.GenerateOne();
        existingTodoItem.Owner = _fakers.Person.GenerateOne();
        existingTodoItem.Assignee = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TodoItems.Add(existingTodoItem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = (object?)null
        };

        string route = $"/todoItems/{existingTodoItem.StringId}/relationships/assignee";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            TodoItem todoItemInDatabase = await dbContext.TodoItems.Include(todoItem => todoItem.Assignee).FirstWithIdAsync(existingTodoItem.Id);

            todoItemInDatabase.Assignee.Should().BeNull();
        });

        store.SqlCommands.Should().HaveCount(2);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."CreatedAt", t1."Description", t1."DurationInHours", t1."LastModifiedAt", t1."Priority", t2."Id", t2."FirstName", t2."LastName"
                FROM "TodoItems" AS t1
                LEFT JOIN "People" AS t2 ON t1."AssigneeId" = t2."Id"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingTodoItem.Id);
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
            command.Parameters.Should().Contain("@p2", existingTodoItem.Id);
        });
    }

    [Fact]
    public async Task Cannot_clear_ManyToOne_relationship_with_required_foreign_key()
    {
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        TodoItem existingTodoItem = _fakers.TodoItem.GenerateOne();
        existingTodoItem.Owner = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TodoItems.Add(existingTodoItem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = (object?)null
        };

        string route = $"/todoItems/{existingTodoItem.StringId}/relationships/owner";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Failed to clear a required relationship.");
        error.Detail.Should().Be("The relationship 'owner' on resource type 'todoItems' cannot be cleared because it is a required relationship.");
        error.Source.Should().BeNull();

        store.SqlCommands.Should().HaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."CreatedAt", t1."Description", t1."DurationInHours", t1."LastModifiedAt", t1."Priority", t2."Id", t2."FirstName", t2."LastName"
                FROM "TodoItems" AS t1
                INNER JOIN "People" AS t2 ON t1."OwnerId" = t2."Id"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingTodoItem.Id);
        });
    }

    [Fact]
    public async Task Can_create_OneToOne_relationship_with_nullable_foreign_key_at_left_side()
    {
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person existingPerson = _fakers.Person.GenerateOne();

        LoginAccount existingLoginAccount = _fakers.LoginAccount.GenerateOne();
        existingLoginAccount.Recovery = _fakers.AccountRecovery.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingPerson, existingLoginAccount);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "loginAccounts",
                id = existingLoginAccount.StringId
            }
        };

        string route = $"/people/{existingPerson.StringId}/relationships/account";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Person personInDatabase = await dbContext.People.Include(person => person.Account).FirstWithIdAsync(existingPerson.Id);

            personInDatabase.Account.ShouldNotBeNull();
            personInDatabase.Account.Id.Should().Be(existingLoginAccount.Id);
        });

        store.SqlCommands.Should().HaveCount(3);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."FirstName", t1."LastName", t2."Id", t2."LastUsedAt", t2."UserName"
                FROM "People" AS t1
                LEFT JOIN "LoginAccounts" AS t2 ON t1."AccountId" = t2."Id"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingPerson.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "People"
                SET "AccountId" = @p1
                WHERE "AccountId" = @p2
                """));

            command.Parameters.Should().HaveCount(2);
            command.Parameters.Should().Contain("@p1", null);
            command.Parameters.Should().Contain("@p2", existingLoginAccount.Id);
        });

        store.SqlCommands[2].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "People"
                SET "AccountId" = @p1
                WHERE "Id" = @p2
                """));

            command.Parameters.Should().HaveCount(2);
            command.Parameters.Should().Contain("@p1", existingLoginAccount.Id);
            command.Parameters.Should().Contain("@p2", existingPerson.Id);
        });
    }

    [Fact]
    public async Task Can_create_OneToOne_relationship_with_nullable_foreign_key_at_right_side()
    {
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        LoginAccount existingLoginAccount = _fakers.LoginAccount.GenerateOne();
        existingLoginAccount.Recovery = _fakers.AccountRecovery.GenerateOne();

        Person existingPerson = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingLoginAccount, existingPerson);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "people",
                id = existingPerson.StringId
            }
        };

        string route = $"/loginAccounts/{existingLoginAccount.StringId}/relationships/person";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            LoginAccount loginAccountInDatabase =
                await dbContext.LoginAccounts.Include(loginAccount => loginAccount.Person).FirstWithIdAsync(existingLoginAccount.Id);

            loginAccountInDatabase.Person.ShouldNotBeNull();
            loginAccountInDatabase.Person.Id.Should().Be(existingPerson.Id);
        });

        store.SqlCommands.Should().HaveCount(2);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."LastUsedAt", t1."UserName", t2."Id", t2."FirstName", t2."LastName"
                FROM "LoginAccounts" AS t1
                LEFT JOIN "People" AS t2 ON t1."Id" = t2."AccountId"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingLoginAccount.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "People"
                SET "AccountId" = @p1
                WHERE "Id" = @p2
                """));

            command.Parameters.Should().HaveCount(2);
            command.Parameters.Should().Contain("@p1", existingLoginAccount.Id);
            command.Parameters.Should().Contain("@p2", existingPerson.Id);
        });
    }

    [Fact]
    public async Task Can_create_ManyToOne_relationship_with_nullable_foreign_key()
    {
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        TodoItem existingTodoItem = _fakers.TodoItem.GenerateOne();
        existingTodoItem.Owner = _fakers.Person.GenerateOne();

        Person existingPerson = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingTodoItem, existingPerson);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "people",
                id = existingPerson.StringId
            }
        };

        string route = $"/todoItems/{existingTodoItem.StringId}/relationships/assignee";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            TodoItem todoItemInDatabase = await dbContext.TodoItems.Include(todoItem => todoItem.Assignee).FirstWithIdAsync(existingTodoItem.Id);

            todoItemInDatabase.Assignee.ShouldNotBeNull();
            todoItemInDatabase.Assignee.Id.Should().Be(existingPerson.Id);
        });

        store.SqlCommands.Should().HaveCount(2);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."CreatedAt", t1."Description", t1."DurationInHours", t1."LastModifiedAt", t1."Priority", t2."Id", t2."FirstName", t2."LastName"
                FROM "TodoItems" AS t1
                LEFT JOIN "People" AS t2 ON t1."AssigneeId" = t2."Id"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingTodoItem.Id);
        });

        store.SqlCommands[1].With(command =>
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
    public async Task Can_replace_OneToOne_relationship_with_nullable_foreign_key_at_left_side()
    {
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person existingPerson1 = _fakers.Person.GenerateOne();
        existingPerson1.Account = _fakers.LoginAccount.GenerateOne();
        existingPerson1.Account.Recovery = _fakers.AccountRecovery.GenerateOne();

        Person existingPerson2 = _fakers.Person.GenerateOne();
        existingPerson2.Account = _fakers.LoginAccount.GenerateOne();
        existingPerson2.Account.Recovery = _fakers.AccountRecovery.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.People.AddRange(existingPerson1, existingPerson2);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "loginAccounts",
                id = existingPerson2.Account.StringId
            }
        };

        string route = $"/people/{existingPerson1.StringId}/relationships/account";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Person personInDatabase1 = await dbContext.People.Include(person => person.Account).FirstWithIdAsync(existingPerson1.Id);

            personInDatabase1.Account.ShouldNotBeNull();
            personInDatabase1.Account.Id.Should().Be(existingPerson2.Account.Id);

            Person personInDatabase2 = await dbContext.People.Include(person => person.Account).FirstWithIdAsync(existingPerson2.Id);

            personInDatabase2.Account.Should().BeNull();
        });

        store.SqlCommands.Should().HaveCount(3);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."FirstName", t1."LastName", t2."Id", t2."LastUsedAt", t2."UserName"
                FROM "People" AS t1
                LEFT JOIN "LoginAccounts" AS t2 ON t1."AccountId" = t2."Id"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingPerson1.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "People"
                SET "AccountId" = @p1
                WHERE "AccountId" = @p2
                """));

            command.Parameters.Should().HaveCount(2);
            command.Parameters.Should().Contain("@p1", null);
            command.Parameters.Should().Contain("@p2", existingPerson2.Account.Id);
        });

        store.SqlCommands[2].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "People"
                SET "AccountId" = @p1
                WHERE "Id" = @p2
                """));

            command.Parameters.Should().HaveCount(2);
            command.Parameters.Should().Contain("@p1", existingPerson2.Account.Id);
            command.Parameters.Should().Contain("@p2", existingPerson1.Id);
        });
    }

    [Fact]
    public async Task Can_replace_OneToOne_relationship_with_nullable_foreign_key_at_right_side()
    {
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        LoginAccount existingLoginAccount1 = _fakers.LoginAccount.GenerateOne();
        existingLoginAccount1.Recovery = _fakers.AccountRecovery.GenerateOne();
        existingLoginAccount1.Person = _fakers.Person.GenerateOne();

        LoginAccount existingLoginAccount2 = _fakers.LoginAccount.GenerateOne();
        existingLoginAccount2.Recovery = _fakers.AccountRecovery.GenerateOne();
        existingLoginAccount2.Person = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.LoginAccounts.AddRange(existingLoginAccount1, existingLoginAccount2);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "people",
                id = existingLoginAccount2.Person.StringId
            }
        };

        string route = $"/loginAccounts/{existingLoginAccount1.StringId}/relationships/person";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            LoginAccount loginAccountInDatabase1 =
                await dbContext.LoginAccounts.Include(loginAccount => loginAccount.Person).FirstWithIdAsync(existingLoginAccount1.Id);

            loginAccountInDatabase1.Person.ShouldNotBeNull();
            loginAccountInDatabase1.Person.Id.Should().Be(existingLoginAccount2.Person.Id);

            LoginAccount loginAccountInDatabase2 =
                await dbContext.LoginAccounts.Include(loginAccount => loginAccount.Person).FirstWithIdAsync(existingLoginAccount2.Id);

            loginAccountInDatabase2.Person.Should().BeNull();
        });

        store.SqlCommands.Should().HaveCount(3);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."LastUsedAt", t1."UserName", t2."Id", t2."FirstName", t2."LastName"
                FROM "LoginAccounts" AS t1
                LEFT JOIN "People" AS t2 ON t1."Id" = t2."AccountId"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingLoginAccount1.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "People"
                SET "AccountId" = @p1
                WHERE "Id" = @p2
                """));

            command.Parameters.Should().HaveCount(2);
            command.Parameters.Should().Contain("@p1", null);
            command.Parameters.Should().Contain("@p2", existingLoginAccount1.Person.Id);
        });

        store.SqlCommands[2].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "People"
                SET "AccountId" = @p1
                WHERE "Id" = @p2
                """));

            command.Parameters.Should().HaveCount(2);
            command.Parameters.Should().Contain("@p1", existingLoginAccount1.Id);
            command.Parameters.Should().Contain("@p2", existingLoginAccount2.Person.Id);
        });
    }

    [Fact]
    public async Task Can_replace_OneToOne_relationship_with_required_foreign_key_at_left_side()
    {
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        LoginAccount existingLoginAccount1 = _fakers.LoginAccount.GenerateOne();
        existingLoginAccount1.Recovery = _fakers.AccountRecovery.GenerateOne();

        LoginAccount existingLoginAccount2 = _fakers.LoginAccount.GenerateOne();
        existingLoginAccount2.Recovery = _fakers.AccountRecovery.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.LoginAccounts.AddRange(existingLoginAccount1, existingLoginAccount2);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "accountRecoveries",
                id = existingLoginAccount2.Recovery.StringId
            }
        };

        string route = $"/loginAccounts/{existingLoginAccount1.StringId}/relationships/recovery";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            LoginAccount loginAccountInDatabase1 =
                await dbContext.LoginAccounts.Include(loginAccount => loginAccount.Recovery).FirstWithIdAsync(existingLoginAccount1.Id);

            loginAccountInDatabase1.Recovery.ShouldNotBeNull();
            loginAccountInDatabase1.Recovery.Id.Should().Be(existingLoginAccount2.Recovery.Id);

            LoginAccount? loginAccountInDatabase2 = await dbContext.LoginAccounts.Include(loginAccount => loginAccount.Recovery)
                .FirstWithIdOrDefaultAsync(existingLoginAccount2.Id);

            loginAccountInDatabase2.Should().BeNull();
        });

        store.SqlCommands.Should().HaveCount(3);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."LastUsedAt", t1."UserName", t2."Id", t2."EmailAddress", t2."PhoneNumber"
                FROM "LoginAccounts" AS t1
                INNER JOIN "AccountRecoveries" AS t2 ON t1."RecoveryId" = t2."Id"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingLoginAccount1.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                DELETE FROM "LoginAccounts"
                WHERE "RecoveryId" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingLoginAccount2.Recovery.Id);
        });

        store.SqlCommands[2].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "LoginAccounts"
                SET "RecoveryId" = @p1
                WHERE "Id" = @p2
                """));

            command.Parameters.Should().HaveCount(2);
            command.Parameters.Should().Contain("@p1", existingLoginAccount2.Recovery.Id);
            command.Parameters.Should().Contain("@p2", existingLoginAccount1.Id);
        });
    }

    [Fact]
    public async Task Can_replace_OneToOne_relationship_with_required_foreign_key_at_right_side()
    {
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        AccountRecovery existingAccountRecovery1 = _fakers.AccountRecovery.GenerateOne();
        existingAccountRecovery1.Account = _fakers.LoginAccount.GenerateOne();

        AccountRecovery existingAccountRecovery2 = _fakers.AccountRecovery.GenerateOne();
        existingAccountRecovery2.Account = _fakers.LoginAccount.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AccountRecoveries.AddRange(existingAccountRecovery1, existingAccountRecovery2);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "loginAccounts",
                id = existingAccountRecovery2.Account.StringId
            }
        };

        string route = $"/accountRecoveries/{existingAccountRecovery1.StringId}/relationships/account";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            AccountRecovery accountRecoveryInDatabase1 =
                await dbContext.AccountRecoveries.Include(recovery => recovery.Account).FirstWithIdAsync(existingAccountRecovery1.Id);

            accountRecoveryInDatabase1.Account.ShouldNotBeNull();
            accountRecoveryInDatabase1.Account.Id.Should().Be(existingAccountRecovery2.Account.Id);

            AccountRecovery accountRecoveryInDatabase2 =
                await dbContext.AccountRecoveries.Include(recovery => recovery.Account).FirstWithIdAsync(existingAccountRecovery2.Id);

            accountRecoveryInDatabase2.Account.Should().BeNull();
        });

        store.SqlCommands.Should().HaveCount(3);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."EmailAddress", t1."PhoneNumber", t2."Id", t2."LastUsedAt", t2."UserName"
                FROM "AccountRecoveries" AS t1
                LEFT JOIN "LoginAccounts" AS t2 ON t1."Id" = t2."RecoveryId"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingAccountRecovery1.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                DELETE FROM "LoginAccounts"
                WHERE "Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingAccountRecovery1.Account.Id);
        });

        store.SqlCommands[2].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "LoginAccounts"
                SET "RecoveryId" = @p1
                WHERE "Id" = @p2
                """));

            command.Parameters.Should().HaveCount(2);
            command.Parameters.Should().Contain("@p1", existingAccountRecovery1.Id);
            command.Parameters.Should().Contain("@p2", existingAccountRecovery2.Account.Id);
        });
    }

    [Fact]
    public async Task Can_replace_ManyToOne_relationship_with_nullable_foreign_key()
    {
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        TodoItem existingTodoItem1 = _fakers.TodoItem.GenerateOne();
        existingTodoItem1.Owner = _fakers.Person.GenerateOne();
        existingTodoItem1.Assignee = _fakers.Person.GenerateOne();

        TodoItem existingTodoItem2 = _fakers.TodoItem.GenerateOne();
        existingTodoItem2.Owner = _fakers.Person.GenerateOne();
        existingTodoItem2.Assignee = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TodoItems.AddRange(existingTodoItem1, existingTodoItem2);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "people",
                id = existingTodoItem2.Assignee.StringId
            }
        };

        string route = $"/todoItems/{existingTodoItem1.StringId}/relationships/assignee";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            TodoItem todoItemInDatabase1 = await dbContext.TodoItems.Include(todoItem => todoItem.Assignee).FirstWithIdAsync(existingTodoItem1.Id);

            todoItemInDatabase1.Assignee.ShouldNotBeNull();
            todoItemInDatabase1.Assignee.Id.Should().Be(existingTodoItem2.Assignee.Id);
        });

        store.SqlCommands.Should().HaveCount(2);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."CreatedAt", t1."Description", t1."DurationInHours", t1."LastModifiedAt", t1."Priority", t2."Id", t2."FirstName", t2."LastName"
                FROM "TodoItems" AS t1
                LEFT JOIN "People" AS t2 ON t1."AssigneeId" = t2."Id"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingTodoItem1.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "TodoItems"
                SET "AssigneeId" = @p1
                WHERE "Id" = @p2
                """));

            command.Parameters.Should().HaveCount(2);
            command.Parameters.Should().Contain("@p1", existingTodoItem2.Assignee.Id);
            command.Parameters.Should().Contain("@p2", existingTodoItem1.Id);
        });
    }

    [Fact]
    public async Task Can_replace_ManyToOne_relationship_with_required_foreign_key()
    {
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        TodoItem existingTodoItem1 = _fakers.TodoItem.GenerateOne();
        existingTodoItem1.Owner = _fakers.Person.GenerateOne();

        TodoItem existingTodoItem2 = _fakers.TodoItem.GenerateOne();
        existingTodoItem2.Owner = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TodoItems.AddRange(existingTodoItem1, existingTodoItem2);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "people",
                id = existingTodoItem2.Owner.StringId
            }
        };

        string route = $"/todoItems/{existingTodoItem1.StringId}/relationships/owner";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            TodoItem todoItemInDatabase1 = await dbContext.TodoItems.Include(todoItem => todoItem.Owner).FirstWithIdAsync(existingTodoItem1.Id);

            todoItemInDatabase1.Owner.ShouldNotBeNull();
            todoItemInDatabase1.Owner.Id.Should().Be(existingTodoItem2.Owner.Id);
        });

        store.SqlCommands.Should().HaveCount(2);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."CreatedAt", t1."Description", t1."DurationInHours", t1."LastModifiedAt", t1."Priority", t2."Id", t2."FirstName", t2."LastName"
                FROM "TodoItems" AS t1
                INNER JOIN "People" AS t2 ON t1."OwnerId" = t2."Id"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingTodoItem1.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "TodoItems"
                SET "OwnerId" = @p1
                WHERE "Id" = @p2
                """));

            command.Parameters.Should().HaveCount(2);
            command.Parameters.Should().Contain("@p1", existingTodoItem2.Owner.Id);
            command.Parameters.Should().Contain("@p2", existingTodoItem1.Id);
        });
    }
}
