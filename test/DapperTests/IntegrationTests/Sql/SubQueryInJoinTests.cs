using System.Net;
using DapperExample.Models;
using DapperExample.Repositories;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace DapperTests.IntegrationTests.Sql;

public sealed class SubQueryInJoinTests : IClassFixture<DapperTestContext>
{
    private readonly DapperTestContext _testContext;
    private readonly TestFakers _fakers = new();

    public SubQueryInJoinTests(DapperTestContext testContext, ITestOutputHelper testOutputHelper)
    {
        testContext.SetTestOutputHelper(testOutputHelper);
        _testContext = testContext;
    }

    [Fact]
    public async Task Join_with_table_on_ToOne_include()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person person = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.People.Add(person);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/people?include=account";

        // Act
        (HttpResponseMessage httpResponse, string _) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        store.SqlCommands.Should().HaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."FirstName", t1."LastName", t2."Id", t2."LastUsedAt", t2."UserName"
                FROM "People" AS t1
                LEFT JOIN "LoginAccounts" AS t2 ON t1."AccountId" = t2."Id"
                """));

            command.Parameters.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Join_with_table_on_ToMany_include()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person person = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.People.Add(person);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/people?include=ownedTodoItems";

        // Act
        (HttpResponseMessage httpResponse, string _) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        store.SqlCommands.Should().HaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."FirstName", t1."LastName", t2."Id", t2."CreatedAt", t2."Description", t2."DurationInHours", t2."LastModifiedAt", t2."Priority"
                FROM "People" AS t1
                LEFT JOIN "TodoItems" AS t2 ON t1."Id" = t2."OwnerId"
                ORDER BY t2."Priority", t2."LastModifiedAt" DESC
                """));

            command.Parameters.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Join_with_table_on_ToMany_include_with_nested_sort_on_attribute()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person person = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.People.Add(person);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/people?include=ownedTodoItems&sort[ownedTodoItems]=description";

        // Act
        (HttpResponseMessage httpResponse, string _) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        store.SqlCommands.Should().HaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."FirstName", t1."LastName", t2."Id", t2."CreatedAt", t2."Description", t2."DurationInHours", t2."LastModifiedAt", t2."Priority"
                FROM "People" AS t1
                LEFT JOIN "TodoItems" AS t2 ON t1."Id" = t2."OwnerId"
                ORDER BY t2."Description"
                """));

            command.Parameters.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Join_with_table_on_ToMany_include_with_nested_sort_on_count()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person person = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.People.Add(person);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/people?include=ownedTodoItems&sort[ownedTodoItems]=count(tags)";

        // Act
        (HttpResponseMessage httpResponse, string _) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        store.SqlCommands.Should().HaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."FirstName", t1."LastName", t2."Id", t2."CreatedAt", t2."Description", t2."DurationInHours", t2."LastModifiedAt", t2."Priority"
                FROM "People" AS t1
                LEFT JOIN "TodoItems" AS t2 ON t1."Id" = t2."OwnerId"
                ORDER BY (
                    SELECT COUNT(*)
                    FROM "Tags" AS t3
                    WHERE t2."Id" = t3."TodoItemId"
                )
                """));

            command.Parameters.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Join_with_tables_on_includes_with_nested_sorts()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person person = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.People.Add(person);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/people?include=ownedTodoItems.tags&sort[ownedTodoItems]=count(tags)&sort[ownedTodoItems.tags]=-name";

        // Act
        (HttpResponseMessage httpResponse, string _) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        store.SqlCommands.Should().HaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."FirstName", t1."LastName", t2."Id", t2."CreatedAt", t2."Description", t2."DurationInHours", t2."LastModifiedAt", t2."Priority", t4."Id", t4."Name"
                FROM "People" AS t1
                LEFT JOIN "TodoItems" AS t2 ON t1."Id" = t2."OwnerId"
                LEFT JOIN "Tags" AS t4 ON t2."Id" = t4."TodoItemId"
                ORDER BY (
                    SELECT COUNT(*)
                    FROM "Tags" AS t3
                    WHERE t2."Id" = t3."TodoItemId"
                ), t4."Name" DESC
                """));

            command.Parameters.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Join_with_tables_on_includes_with_nested_sorts_on_counts()
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

        const string route =
            "/todoItems?include=owner.ownedTodoItems.tags,owner.assignedTodoItems.tags&sort[owner.ownedTodoItems]=count(tags)&sort[owner.assignedTodoItems]=count(tags)";

        // Act
        (HttpResponseMessage httpResponse, string _) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        store.SqlCommands.Should().HaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."CreatedAt", t1."Description", t1."DurationInHours", t1."LastModifiedAt", t1."Priority", t2."Id", t2."FirstName", t2."LastName", t3."Id", t3."CreatedAt", t3."Description", t3."DurationInHours", t3."LastModifiedAt", t3."Priority", t5."Id", t5."Name", t6."Id", t6."CreatedAt", t6."Description", t6."DurationInHours", t6."LastModifiedAt", t6."Priority", t8."Id", t8."Name"
                FROM "TodoItems" AS t1
                INNER JOIN "People" AS t2 ON t1."OwnerId" = t2."Id"
                LEFT JOIN "TodoItems" AS t3 ON t2."Id" = t3."AssigneeId"
                LEFT JOIN "Tags" AS t5 ON t3."Id" = t5."TodoItemId"
                LEFT JOIN "TodoItems" AS t6 ON t2."Id" = t6."OwnerId"
                LEFT JOIN "Tags" AS t8 ON t6."Id" = t8."TodoItemId"
                ORDER BY t1."Priority", t1."LastModifiedAt" DESC, (
                    SELECT COUNT(*)
                    FROM "Tags" AS t4
                    WHERE t3."Id" = t4."TodoItemId"
                ), (
                    SELECT COUNT(*)
                    FROM "Tags" AS t7
                    WHERE t6."Id" = t7."TodoItemId"
                )
                """));

            command.Parameters.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Join_with_sub_query_on_ToMany_include_with_nested_filter()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person person = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.People.Add(person);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/people?include=ownedTodoItems&filter[ownedTodoItems]=equals(description,'X')";

        // Act
        (HttpResponseMessage httpResponse, string _) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        store.SqlCommands.Should().HaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."FirstName", t1."LastName", t3."Id", t3."CreatedAt", t3."Description", t3."DurationInHours", t3."LastModifiedAt", t3."Priority"
                FROM "People" AS t1
                LEFT JOIN (
                    SELECT t2."Id", t2."CreatedAt", t2."Description", t2."DurationInHours", t2."LastModifiedAt", t2."OwnerId", t2."Priority"
                    FROM "TodoItems" AS t2
                    WHERE t2."Description" = @p1
                ) AS t3 ON t1."Id" = t3."OwnerId"
                ORDER BY t3."Priority", t3."LastModifiedAt" DESC
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", "X");
        });
    }

    [Fact]
    public async Task Join_with_sub_query_on_ToMany_include_with_nested_filter_on_has()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person person = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.People.Add(person);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/people?include=ownedTodoItems&filter[ownedTodoItems]=has(tags)";

        // Act
        (HttpResponseMessage httpResponse, string _) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        store.SqlCommands.Should().HaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."FirstName", t1."LastName", t4."Id", t4."CreatedAt", t4."Description", t4."DurationInHours", t4."LastModifiedAt", t4."Priority"
                FROM "People" AS t1
                LEFT JOIN (
                    SELECT t2."Id", t2."CreatedAt", t2."Description", t2."DurationInHours", t2."LastModifiedAt", t2."OwnerId", t2."Priority"
                    FROM "TodoItems" AS t2
                    WHERE EXISTS (
                        SELECT 1
                        FROM "Tags" AS t3
                        WHERE t2."Id" = t3."TodoItemId"
                    )
                ) AS t4 ON t1."Id" = t4."OwnerId"
                ORDER BY t4."Priority", t4."LastModifiedAt" DESC
                """));

            command.Parameters.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Join_with_sub_query_on_ToMany_include_with_nested_filter_on_count()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person person = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.People.Add(person);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/people?include=ownedTodoItems&filter[ownedTodoItems]=greaterThan(count(tags),'0')";

        // Act
        (HttpResponseMessage httpResponse, string _) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        store.SqlCommands.Should().HaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."FirstName", t1."LastName", t4."Id", t4."CreatedAt", t4."Description", t4."DurationInHours", t4."LastModifiedAt", t4."Priority"
                FROM "People" AS t1
                LEFT JOIN (
                    SELECT t2."Id", t2."CreatedAt", t2."Description", t2."DurationInHours", t2."LastModifiedAt", t2."OwnerId", t2."Priority"
                    FROM "TodoItems" AS t2
                    WHERE (
                        SELECT COUNT(*)
                        FROM "Tags" AS t3
                        WHERE t2."Id" = t3."TodoItemId"
                    ) > @p1
                ) AS t4 ON t1."Id" = t4."OwnerId"
                ORDER BY t4."Priority", t4."LastModifiedAt" DESC
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", 0);
        });
    }

    [Fact]
    public async Task Join_with_sub_query_on_includes_with_nested_filter_and_sorts()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person person = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.People.Add(person);
            await dbContext.SaveChangesAsync();
        });

        const string route =
            "/people?include=ownedTodoItems.tags&filter[ownedTodoItems]=equals(description,'X')&sort[ownedTodoItems]=count(tags)&sort[ownedTodoItems.tags]=-name";

        // Act
        (HttpResponseMessage httpResponse, string _) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        store.SqlCommands.Should().HaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."FirstName", t1."LastName", t5."Id", t5."CreatedAt", t5."Description", t5."DurationInHours", t5."LastModifiedAt", t5."Priority", t5.Id0 AS Id, t5."Name"
                FROM "People" AS t1
                LEFT JOIN (
                    SELECT t2."Id", t2."CreatedAt", t2."Description", t2."DurationInHours", t2."LastModifiedAt", t2."OwnerId", t2."Priority", t4."Id" AS Id0, t4."Name"
                    FROM "TodoItems" AS t2
                    LEFT JOIN "Tags" AS t4 ON t2."Id" = t4."TodoItemId"
                    WHERE t2."Description" = @p1
                ) AS t5 ON t1."Id" = t5."OwnerId"
                ORDER BY (
                    SELECT COUNT(*)
                    FROM "Tags" AS t3
                    WHERE t5."Id" = t3."TodoItemId"
                ), t5."Name" DESC
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", "X");
        });
    }

    [Fact]
    public async Task Join_with_nested_sub_queries_with_filters_and_sorts()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person person = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.People.Add(person);
            await dbContext.SaveChangesAsync();
        });

        const string route =
            "/people?include=ownedTodoItems.tags&filter[ownedTodoItems]=not(equals(description,'X'))&filter[ownedTodoItems.tags]=not(equals(name,'Y'))" +
            "&sort[ownedTodoItems]=count(tags),assignee.lastName&sort[ownedTodoItems.tags]=name,-id";

        // Act
        (HttpResponseMessage httpResponse, string _) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        store.SqlCommands.Should().HaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."FirstName", t1."LastName", t7."Id", t7."CreatedAt", t7."Description", t7."DurationInHours", t7."LastModifiedAt", t7."Priority", t7.Id00 AS Id, t7."Name"
                FROM "People" AS t1
                LEFT JOIN (
                    SELECT t2."Id", t2."CreatedAt", t2."Description", t2."DurationInHours", t2."LastModifiedAt", t2."OwnerId", t2."Priority", t4."LastName", t6."Id" AS Id00, t6."Name"
                    FROM "TodoItems" AS t2
                    LEFT JOIN "People" AS t4 ON t2."AssigneeId" = t4."Id"
                    LEFT JOIN (
                        SELECT t5."Id", t5."Name", t5."TodoItemId"
                        FROM "Tags" AS t5
                        WHERE NOT (t5."Name" = @p2)
                    ) AS t6 ON t2."Id" = t6."TodoItemId"
                    WHERE NOT (t2."Description" = @p1)
                ) AS t7 ON t1."Id" = t7."OwnerId"
                ORDER BY (
                    SELECT COUNT(*)
                    FROM "Tags" AS t3
                    WHERE t7."Id" = t3."TodoItemId"
                ), t7."LastName", t7."Name", t7.Id00 DESC
                """));

            command.Parameters.Should().HaveCount(2);
            command.Parameters.Should().Contain("@p1", "X");
            command.Parameters.Should().Contain("@p2", "Y");
        });
    }
}
