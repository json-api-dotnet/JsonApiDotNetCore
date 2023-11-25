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

public sealed class SparseFieldSets : IClassFixture<DapperTestContext>
{
    private readonly DapperTestContext _testContext;
    private readonly TestFakers _fakers = new();

    public SparseFieldSets(DapperTestContext testContext, ITestOutputHelper testOutputHelper)
    {
        testContext.SetTestOutputHelper(testOutputHelper);
        _testContext = testContext;
    }

    [Fact]
    public async Task Can_select_fields_in_primary_and_included_resources()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        TodoItem todoItem = _fakers.TodoItem.Generate();
        todoItem.Owner = _fakers.Person.Generate();
        todoItem.Assignee = _fakers.Person.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.TodoItems.Add(todoItem);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/todoItems?include=owner,assignee&fields[todoItems]=description,durationInHours,owner,assignee&fields[people]=lastName";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("todoItems");
        responseDocument.Data.ManyValue[0].Id.Should().Be(todoItem.StringId);
        responseDocument.Data.ManyValue[0].Attributes.ShouldHaveCount(2);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("description").With(value => value.Should().Be(todoItem.Description));
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("durationInHours").With(value => value.Should().Be(todoItem.DurationInHours));
        responseDocument.Data.ManyValue[0].Relationships.ShouldHaveCount(2);

        responseDocument.Data.ManyValue[0].Relationships.ShouldContainKey("owner").With(value =>
        {
            value.ShouldNotBeNull();
            value.Data.SingleValue.ShouldNotBeNull();
            value.Data.SingleValue.Type.Should().Be("people");
            value.Data.SingleValue.Id.Should().Be(todoItem.Owner.StringId);
        });

        responseDocument.Data.ManyValue[0].Relationships.ShouldContainKey("assignee").With(value =>
        {
            value.ShouldNotBeNull();
            value.Data.SingleValue.ShouldNotBeNull();
            value.Data.SingleValue.Type.Should().Be("people");
            value.Data.SingleValue.Id.Should().Be(todoItem.Assignee.StringId);
        });

        responseDocument.Included.ShouldHaveCount(2);
        responseDocument.Included.Should().AllSatisfy(resource => resource.Type.Should().Be("people"));

        responseDocument.Included[0].Id.Should().Be(todoItem.Owner.StringId);
        responseDocument.Included[0].Attributes.ShouldHaveCount(1);
        responseDocument.Included[0].Attributes.ShouldContainKey("lastName").With(value => value.Should().Be(todoItem.Owner.LastName));
        responseDocument.Included[0].Relationships.Should().BeNull();

        responseDocument.Included[1].Id.Should().Be(todoItem.Assignee.StringId);
        responseDocument.Included[1].Attributes.ShouldHaveCount(1);
        responseDocument.Included[1].Attributes.ShouldContainKey("lastName").With(value => value.Should().Be(todoItem.Assignee.LastName));
        responseDocument.Included[1].Relationships.Should().BeNull();

        store.SqlCommands.ShouldHaveCount(2);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql(@"SELECT COUNT(*)
FROM ""TodoItems"" AS t1"));

            command.Parameters.Should().BeEmpty();
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql(
                @"SELECT t1.""Id"", t1.""Description"", t1.""DurationInHours"", t2.""Id"", t2.""LastName"", t3.""Id"", t3.""LastName""
FROM ""TodoItems"" AS t1
LEFT JOIN ""People"" AS t2 ON t1.""AssigneeId"" = t2.""Id""
INNER JOIN ""People"" AS t3 ON t1.""OwnerId"" = t3.""Id""
ORDER BY t1.""Priority"", t1.""LastModifiedAt"" DESC"));

            command.Parameters.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Can_select_attribute_in_primary_resource()
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

        string route = $"/todoItems/{todoItem.StringId}?fields[todoItems]=description";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("todoItems");
        responseDocument.Data.SingleValue.Id.Should().Be(todoItem.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldHaveCount(1);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("description").With(value => value.Should().Be(todoItem.Description));
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        store.SqlCommands.ShouldHaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql(@"SELECT t1.""Id"", t1.""Description""
FROM ""TodoItems"" AS t1
WHERE t1.""Id"" = @p1"));

            command.Parameters.ShouldHaveCount(1);
            command.Parameters.Should().Contain("@p1", todoItem.Id);
        });
    }

    [Fact]
    public async Task Can_select_relationship_in_secondary_resources()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        TodoItem todoItem = _fakers.TodoItem.Generate();
        todoItem.Owner = _fakers.Person.Generate();
        todoItem.Tags = _fakers.Tag.Generate(1).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TodoItems.Add(todoItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/todoItems/{todoItem.StringId}/tags?fields[tags]=color";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("tags");
        responseDocument.Data.ManyValue[0].Id.Should().Be(todoItem.Tags.ElementAt(0).StringId);
        responseDocument.Data.ManyValue[0].Attributes.Should().BeNull();
        responseDocument.Data.ManyValue[0].Relationships.ShouldHaveCount(1);

        responseDocument.Data.ManyValue[0].Relationships.ShouldContainKey("color").With(value =>
        {
            value.ShouldNotBeNull();
            value.Data.Value.Should().BeNull();
        });

        store.SqlCommands.ShouldHaveCount(2);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql(@"SELECT COUNT(*)
FROM ""Tags"" AS t1
LEFT JOIN ""TodoItems"" AS t2 ON t1.""TodoItemId"" = t2.""Id""
WHERE t2.""Id"" = @p1"));

            command.Parameters.ShouldHaveCount(1);
            command.Parameters.Should().Contain("@p1", todoItem.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql(@"SELECT t1.""Id"", t2.""Id""
FROM ""TodoItems"" AS t1
LEFT JOIN ""Tags"" AS t2 ON t1.""Id"" = t2.""TodoItemId""
WHERE t1.""Id"" = @p1
ORDER BY t2.""Id"""));

            command.Parameters.ShouldHaveCount(1);
            command.Parameters.Should().Contain("@p1", todoItem.Id);
        });
    }

    [Fact]
    public async Task Can_select_ID()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person person = _fakers.Person.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.People.Add(person);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/people/{person.StringId}?fields[people]=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("people");
        responseDocument.Data.SingleValue.Id.Should().Be(person.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().BeNull();
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        store.SqlCommands.ShouldHaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql(@"SELECT t1.""Id""
FROM ""People"" AS t1
WHERE t1.""Id"" = @p1"));

            command.Parameters.ShouldHaveCount(1);
            command.Parameters.Should().Contain("@p1", person.Id);
        });
    }

    [Fact]
    public async Task Can_select_empty_fieldset()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person person = _fakers.Person.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.People.Add(person);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/people/{person.StringId}?fields[people]=";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("people");
        responseDocument.Data.SingleValue.Id.Should().Be(person.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().BeNull();
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        store.SqlCommands.ShouldHaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql(@"SELECT t1.""Id""
FROM ""People"" AS t1
WHERE t1.""Id"" = @p1"));

            command.Parameters.ShouldHaveCount(1);
            command.Parameters.Should().Contain("@p1", person.Id);
        });
    }

    [Fact]
    public async Task Fetches_all_scalar_properties_when_fieldset_contains_readonly_attribute()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person person = _fakers.Person.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.People.Add(person);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/people/{person.StringId}?fields[people]=displayName";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("people");
        responseDocument.Data.SingleValue.Id.Should().Be(person.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldHaveCount(1);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("displayName").With(value => value.Should().Be(person.DisplayName));
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        store.SqlCommands.ShouldHaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql(@"SELECT t1.""Id"", t1.""FirstName"", t1.""LastName""
FROM ""People"" AS t1
WHERE t1.""Id"" = @p1"));

            command.Parameters.ShouldHaveCount(1);
            command.Parameters.Should().Contain("@p1", person.Id);
        });
    }

    [Fact]
    public async Task Returns_related_resources_on_broken_resource_linkage()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        TodoItem todoItem = _fakers.TodoItem.Generate();
        todoItem.Owner = _fakers.Person.Generate();
        todoItem.Tags = _fakers.Tag.Generate(2).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TodoItems.Add(todoItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/todoItems/{todoItem.StringId}?include=tags&fields[todoItems]=description";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("todoItems");
        responseDocument.Data.SingleValue.Id.Should().Be(todoItem.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldHaveCount(1);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("description").With(value => value.Should().Be(todoItem.Description));
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        responseDocument.Included.ShouldHaveCount(2);
        responseDocument.Included.Should().AllSatisfy(resource => resource.Type.Should().Be("tags"));

        store.SqlCommands.ShouldHaveCount(1);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql(@"SELECT t1.""Id"", t1.""Description"", t2.""Id"", t2.""Name""
FROM ""TodoItems"" AS t1
LEFT JOIN ""Tags"" AS t2 ON t1.""Id"" = t2.""TodoItemId""
WHERE t1.""Id"" = @p1
ORDER BY t2.""Id"""));

            command.Parameters.ShouldHaveCount(1);
            command.Parameters.Should().Contain("@p1", todoItem.Id);
        });
    }
}
