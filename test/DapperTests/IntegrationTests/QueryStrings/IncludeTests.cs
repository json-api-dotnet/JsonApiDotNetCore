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

public sealed class IncludeTests : IClassFixture<DapperTestContext>
{
    private readonly DapperTestContext _testContext;
    private readonly TestFakers _fakers = new();

    public IncludeTests(DapperTestContext testContext, ITestOutputHelper testOutputHelper)
    {
        testContext.SetTestOutputHelper(testOutputHelper);
        _testContext = testContext;
    }

    [Fact]
    public async Task Can_get_primary_resources_with_multiple_include_chains()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person owner = _fakers.Person.GenerateOne();

        List<TodoItem> todoItems = _fakers.TodoItem.GenerateList(2);
        todoItems.ForEach(todoItem => todoItem.Owner = owner);
        todoItems.ForEach(todoItem => todoItem.Tags = _fakers.Tag.GenerateSet(2));
        todoItems[1].Assignee = _fakers.Person.GenerateOne();

        todoItems[0].Priority = TodoItemPriority.High;
        todoItems[1].Priority = TodoItemPriority.Low;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.TodoItems.AddRange(todoItems);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/todoItems?include=owner.assignedTodoItems,assignee,tags";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue.Should().AllSatisfy(resource => resource.Type.Should().Be("todoItems"));

        responseDocument.Data.ManyValue[0].Id.Should().Be(todoItems[0].StringId);

        responseDocument.Data.ManyValue[0].Relationships.With(relationships =>
        {
            relationships.Should().ContainKey("owner").WhoseValue.With(value =>
            {
                value.Should().NotBeNull();
                value.Data.SingleValue.Should().NotBeNull();
                value.Data.SingleValue.Type.Should().Be("people");
                value.Data.SingleValue.Id.Should().Be(todoItems[0].Owner.StringId);
            });

            relationships.Should().ContainKey("assignee").WhoseValue.With(value =>
            {
                value.Should().NotBeNull();
                value.Data.SingleValue.Should().BeNull();
            });

            relationships.Should().ContainKey("tags").WhoseValue.With(value =>
            {
                value.Should().NotBeNull();
                value.Data.ManyValue.Should().HaveCount(2);
                value.Data.ManyValue.Should().AllSatisfy(resource => resource.Type.Should().Be("tags"));
                value.Data.ManyValue.Should().ContainSingle(resource => resource.Id == todoItems[0].Tags.ElementAt(0).StringId);
                value.Data.ManyValue.Should().ContainSingle(resource => resource.Id == todoItems[0].Tags.ElementAt(1).StringId);
            });
        });

        responseDocument.Data.ManyValue[1].Id.Should().Be(todoItems[1].StringId);

        responseDocument.Data.ManyValue[1].Relationships.With(relationships =>
        {
            relationships.Should().ContainKey("owner").WhoseValue.With(value =>
            {
                value.Should().NotBeNull();
                value.Data.SingleValue.Should().NotBeNull();
                value.Data.SingleValue.Type.Should().Be("people");
                value.Data.SingleValue.Id.Should().Be(todoItems[1].Owner.StringId);
            });

            relationships.Should().ContainKey("assignee").WhoseValue.With(value =>
            {
                value.Should().NotBeNull();
                value.Data.SingleValue.Should().NotBeNull();
                value.Data.SingleValue.Type.Should().Be("people");
                value.Data.SingleValue.Id.Should().Be(todoItems[1].Assignee!.StringId);
            });

            relationships.Should().ContainKey("tags").WhoseValue.With(value =>
            {
                value.Should().NotBeNull();
                value.Data.ManyValue.Should().HaveCount(2);
                value.Data.ManyValue.Should().AllSatisfy(resource => resource.Type.Should().Be("tags"));
                value.Data.ManyValue.Should().ContainSingle(resource => resource.Id == todoItems[1].Tags.ElementAt(0).StringId);
                value.Data.ManyValue.Should().ContainSingle(resource => resource.Id == todoItems[1].Tags.ElementAt(1).StringId);
            });
        });

        responseDocument.Included.Should().HaveCount(6);

        responseDocument.Included[0].Type.Should().Be("people");
        responseDocument.Included[0].Id.Should().Be(owner.StringId);
        responseDocument.Included[0].Attributes.Should().ContainKey("firstName").WhoseValue.Should().Be(owner.FirstName);
        responseDocument.Included[0].Attributes.Should().ContainKey("lastName").WhoseValue.Should().Be(owner.LastName);

        responseDocument.Included[1].Type.Should().Be("tags");
        responseDocument.Included[1].Id.Should().Be(todoItems[0].Tags.ElementAt(0).StringId);
        responseDocument.Included[1].Attributes.Should().ContainKey("name").WhoseValue.Should().Be(todoItems[0].Tags.ElementAt(0).Name);

        responseDocument.Included[2].Type.Should().Be("tags");
        responseDocument.Included[2].Id.Should().Be(todoItems[0].Tags.ElementAt(1).StringId);
        responseDocument.Included[2].Attributes.Should().ContainKey("name").WhoseValue.Should().Be(todoItems[0].Tags.ElementAt(1).Name);

        responseDocument.Included[3].Type.Should().Be("people");
        responseDocument.Included[3].Id.Should().Be(todoItems[1].Assignee!.StringId);
        responseDocument.Included[3].Attributes.Should().ContainKey("firstName").WhoseValue.Should().Be(todoItems[1].Assignee!.FirstName);
        responseDocument.Included[3].Attributes.Should().ContainKey("lastName").WhoseValue.Should().Be(todoItems[1].Assignee!.LastName);

        responseDocument.Included[4].Type.Should().Be("tags");
        responseDocument.Included[4].Id.Should().Be(todoItems[1].Tags.ElementAt(0).StringId);
        responseDocument.Included[4].Attributes.Should().ContainKey("name").WhoseValue.Should().Be(todoItems[1].Tags.ElementAt(0).Name);

        responseDocument.Included[5].Type.Should().Be("tags");
        responseDocument.Included[5].Id.Should().Be(todoItems[1].Tags.ElementAt(1).StringId);
        responseDocument.Included[5].Attributes.Should().ContainKey("name").WhoseValue.Should().Be(todoItems[1].Tags.ElementAt(1).Name);

        responseDocument.Meta.Should().ContainTotal(2);

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
                SELECT t1."Id", t1."CreatedAt", t1."Description", t1."DurationInHours", t1."LastModifiedAt", t1."Priority", t2."Id", t2."FirstName", t2."LastName", t3."Id", t3."FirstName", t3."LastName", t4."Id", t4."CreatedAt", t4."Description", t4."DurationInHours", t4."LastModifiedAt", t4."Priority", t5."Id", t5."Name"
                FROM "TodoItems" AS t1
                LEFT JOIN "People" AS t2 ON t1."AssigneeId" = t2."Id"
                INNER JOIN "People" AS t3 ON t1."OwnerId" = t3."Id"
                LEFT JOIN "TodoItems" AS t4 ON t3."Id" = t4."AssigneeId"
                LEFT JOIN "Tags" AS t5 ON t1."Id" = t5."TodoItemId"
                ORDER BY t1."Priority", t1."LastModifiedAt" DESC, t4."Priority", t4."LastModifiedAt" DESC
                """));

            command.Parameters.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Can_get_primary_resources_with_includes()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        List<TodoItem> todoItems = _fakers.TodoItem.GenerateList(25);
        todoItems.ForEach(todoItem => todoItem.Owner = _fakers.Person.GenerateOne());
        todoItems.ForEach(todoItem => todoItem.Tags = _fakers.Tag.GenerateSet(15));
        todoItems.ForEach(todoItem => todoItem.Tags.ForEach(tag => tag.Color = _fakers.RgbColor.GenerateOne()));

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.TodoItems.AddRange(todoItems);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/todoItems?include=tags.color";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(25);

        responseDocument.Data.ManyValue.ForEach(resource =>
        {
            resource.Type.Should().Be("todoItems");
            resource.Attributes.Should().OnlyContainKeys("description", "priority", "durationInHours", "createdAt", "modifiedAt");
            resource.Relationships.Should().OnlyContainKeys("owner", "assignee", "tags");
        });

        responseDocument.Included.Should().HaveCount(25 * 15 * 2);

        responseDocument.Meta.Should().ContainTotal(25);

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
                SELECT t1."Id", t1."CreatedAt", t1."Description", t1."DurationInHours", t1."LastModifiedAt", t1."Priority", t2."Id", t2."Name", t3."Id"
                FROM "TodoItems" AS t1
                LEFT JOIN "Tags" AS t2 ON t1."Id" = t2."TodoItemId"
                LEFT JOIN "RgbColors" AS t3 ON t2."Id" = t3."TagId"
                ORDER BY t1."Priority", t1."LastModifiedAt" DESC
                """));

            command.Parameters.Should().BeEmpty();
        });
    }
}
