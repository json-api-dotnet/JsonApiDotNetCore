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

namespace DapperTests.IntegrationTests.ReadWrite.Resources;

public sealed class UpdateResourceTests : IClassFixture<DapperTestContext>
{
    private readonly DapperTestContext _testContext;
    private readonly TestFakers _fakers = new();

    public UpdateResourceTests(DapperTestContext testContext, ITestOutputHelper testOutputHelper)
    {
        testContext.SetTestOutputHelper(testOutputHelper);
        _testContext = testContext;
    }

    [Fact]
    public async Task Can_update_resource_without_attributes_or_relationships()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Tag existingTag = _fakers.Tag.GenerateOne();
        existingTag.Color = _fakers.RgbColor.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.Tags.Add(existingTag);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "tags",
                id = existingTag.StringId,
                attributes = new
                {
                },
                relationships = new
                {
                }
            }
        };

        string route = $"/tags/{existingTag.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Tag tagInDatabase = await dbContext.Tags.Include(tag => tag.Color).FirstWithIdAsync(existingTag.Id);

            tagInDatabase.Name.Should().Be(existingTag.Name);
            tagInDatabase.Color.ShouldNotBeNull();
            tagInDatabase.Color.Id.Should().Be(existingTag.Color.Id);
        });

        store.SqlCommands.Should().HaveCount(2);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."Name"
                FROM "Tags" AS t1
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingTag.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."Name"
                FROM "Tags" AS t1
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingTag.Id);
        });
    }

    [Fact]
    public async Task Can_partially_update_resource_attributes()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        TodoItem existingTodoItem = _fakers.TodoItem.GenerateOne();
        existingTodoItem.Owner = _fakers.Person.GenerateOne();
        existingTodoItem.Assignee = _fakers.Person.GenerateOne();
        existingTodoItem.Tags = _fakers.Tag.GenerateSet(1);

        string newDescription = _fakers.TodoItem.GenerateOne().Description;
        long newDurationInHours = _fakers.TodoItem.GenerateOne().DurationInHours!.Value;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TodoItems.Add(existingTodoItem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "todoItems",
                id = existingTodoItem.StringId,
                attributes = new
                {
                    description = newDescription,
                    durationInHours = newDurationInHours
                }
            }
        };

        string route = $"/todoItems/{existingTodoItem.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("todoItems");
        responseDocument.Data.SingleValue.Id.Should().Be(existingTodoItem.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("description").With(value => value.Should().Be(newDescription));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("priority").With(value => value.Should().Be(existingTodoItem.Priority));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("durationInHours").With(value => value.Should().Be(newDurationInHours));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("createdAt").With(value => value.Should().Be(existingTodoItem.CreatedAt));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("modifiedAt").With(value => value.Should().Be(DapperTestContext.FrozenTime));
        responseDocument.Data.SingleValue.Relationships.Should().OnlyContainKeys("owner", "assignee", "tags");

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            TodoItem todoItemInDatabase = await dbContext.TodoItems
                .Include(todoItem => todoItem.Owner)
                .Include(todoItem => todoItem.Assignee)
                .Include(todoItem => todoItem.Tags)
                .FirstWithIdAsync(existingTodoItem.Id);

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            todoItemInDatabase.Description.Should().Be(newDescription);
            todoItemInDatabase.Priority.Should().Be(existingTodoItem.Priority);
            todoItemInDatabase.DurationInHours.Should().Be(newDurationInHours);
            todoItemInDatabase.CreatedAt.Should().Be(existingTodoItem.CreatedAt);
            todoItemInDatabase.LastModifiedAt.Should().Be(DapperTestContext.FrozenTime);

            todoItemInDatabase.Owner.ShouldNotBeNull();
            todoItemInDatabase.Owner.Id.Should().Be(existingTodoItem.Owner.Id);
            todoItemInDatabase.Assignee.ShouldNotBeNull();
            todoItemInDatabase.Assignee.Id.Should().Be(existingTodoItem.Assignee.Id);
            todoItemInDatabase.Tags.Should().HaveCount(1);
            todoItemInDatabase.Tags.ElementAt(0).Id.Should().Be(existingTodoItem.Tags.ElementAt(0).Id);
        });

        store.SqlCommands.Should().HaveCount(3);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."CreatedAt", t1."Description", t1."DurationInHours", t1."LastModifiedAt", t1."Priority"
                FROM "TodoItems" AS t1
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingTodoItem.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "TodoItems"
                SET "Description" = @p1, "DurationInHours" = @p2, "LastModifiedAt" = @p3
                WHERE "Id" = @p4
                """));

            command.Parameters.Should().HaveCount(4);
            command.Parameters.Should().Contain("@p1", newDescription);
            command.Parameters.Should().Contain("@p2", newDurationInHours);
            command.Parameters.Should().Contain("@p3", DapperTestContext.FrozenTime);
            command.Parameters.Should().Contain("@p4", existingTodoItem.Id);
        });

        store.SqlCommands[2].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."CreatedAt", t1."Description", t1."DurationInHours", t1."LastModifiedAt", t1."Priority"
                FROM "TodoItems" AS t1
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingTodoItem.Id);
        });
    }

    [Fact]
    public async Task Can_completely_update_resource()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        TodoItem existingTodoItem = _fakers.TodoItem.GenerateOne();
        existingTodoItem.Owner = _fakers.Person.GenerateOne();
        existingTodoItem.Assignee = _fakers.Person.GenerateOne();
        existingTodoItem.Tags = _fakers.Tag.GenerateSet(2);

        TodoItem newTodoItem = _fakers.TodoItem.GenerateOne();

        Tag existingTag = _fakers.Tag.GenerateOne();
        Person existingPerson1 = _fakers.Person.GenerateOne();
        Person existingPerson2 = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingTodoItem, existingTag, existingPerson1, existingPerson2);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "todoItems",
                id = existingTodoItem.StringId,
                attributes = new
                {
                    description = newTodoItem.Description,
                    priority = newTodoItem.Priority,
                    durationInHours = newTodoItem.DurationInHours
                },
                relationships = new
                {
                    owner = new
                    {
                        data = new
                        {
                            type = "people",
                            id = existingPerson1.StringId
                        }
                    },
                    assignee = new
                    {
                        data = new
                        {
                            type = "people",
                            id = existingPerson2.StringId
                        }
                    },
                    tags = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "tags",
                                id = existingTag.StringId
                            }
                        }
                    }
                }
            }
        };

        string route = $"/todoItems/{existingTodoItem.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("todoItems");
        responseDocument.Data.SingleValue.Id.Should().Be(existingTodoItem.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("description").With(value => value.Should().Be(newTodoItem.Description));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("priority").With(value => value.Should().Be(newTodoItem.Priority));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("durationInHours").With(value => value.Should().Be(newTodoItem.DurationInHours));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("createdAt").With(value => value.Should().Be(existingTodoItem.CreatedAt));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("modifiedAt").With(value => value.Should().Be(DapperTestContext.FrozenTime));
        responseDocument.Data.SingleValue.Relationships.Should().OnlyContainKeys("owner", "assignee", "tags");

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            TodoItem todoItemInDatabase = await dbContext.TodoItems
                .Include(todoItem => todoItem.Owner)
                .Include(todoItem => todoItem.Assignee)
                .Include(todoItem => todoItem.Tags)
                .FirstWithIdAsync(existingTodoItem.Id);

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            todoItemInDatabase.Description.Should().Be(newTodoItem.Description);
            todoItemInDatabase.Priority.Should().Be(newTodoItem.Priority);
            todoItemInDatabase.DurationInHours.Should().Be(newTodoItem.DurationInHours);
            todoItemInDatabase.CreatedAt.Should().Be(existingTodoItem.CreatedAt);
            todoItemInDatabase.LastModifiedAt.Should().Be(DapperTestContext.FrozenTime);

            todoItemInDatabase.Owner.ShouldNotBeNull();
            todoItemInDatabase.Owner.Id.Should().Be(existingPerson1.Id);
            todoItemInDatabase.Assignee.ShouldNotBeNull();
            todoItemInDatabase.Assignee.Id.Should().Be(existingPerson2.Id);
            todoItemInDatabase.Tags.Should().HaveCount(1);
            todoItemInDatabase.Tags.ElementAt(0).Id.Should().Be(existingTag.Id);
        });

        store.SqlCommands.Should().HaveCount(5);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."CreatedAt", t1."Description", t1."DurationInHours", t1."LastModifiedAt", t1."Priority", t2."Id", t2."FirstName", t2."LastName", t3."Id", t3."FirstName", t3."LastName", t4."Id", t4."Name"
                FROM "TodoItems" AS t1
                LEFT JOIN "People" AS t2 ON t1."AssigneeId" = t2."Id"
                INNER JOIN "People" AS t3 ON t1."OwnerId" = t3."Id"
                LEFT JOIN "Tags" AS t4 ON t1."Id" = t4."TodoItemId"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingTodoItem.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "TodoItems"
                SET "Description" = @p1, "Priority" = @p2, "DurationInHours" = @p3, "LastModifiedAt" = @p4, "OwnerId" = @p5, "AssigneeId" = @p6
                WHERE "Id" = @p7
                """));

            command.Parameters.Should().HaveCount(7);
            command.Parameters.Should().Contain("@p1", newTodoItem.Description);
            command.Parameters.Should().Contain("@p2", newTodoItem.Priority);
            command.Parameters.Should().Contain("@p3", newTodoItem.DurationInHours);
            command.Parameters.Should().Contain("@p4", DapperTestContext.FrozenTime);
            command.Parameters.Should().Contain("@p5", existingPerson1.Id);
            command.Parameters.Should().Contain("@p6", existingPerson2.Id);
            command.Parameters.Should().Contain("@p7", existingTodoItem.Id);
        });

        store.SqlCommands[2].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "Tags"
                SET "TodoItemId" = @p1
                WHERE "Id" IN (@p2, @p3)
                """));

            command.Parameters.Should().HaveCount(3);
            command.Parameters.Should().Contain("@p1", null);
            command.Parameters.Should().Contain("@p2", existingTodoItem.Tags.ElementAt(0).Id);
            command.Parameters.Should().Contain("@p3", existingTodoItem.Tags.ElementAt(1).Id);
        });

        store.SqlCommands[3].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "Tags"
                SET "TodoItemId" = @p1
                WHERE "Id" = @p2
                """));

            command.Parameters.Should().HaveCount(2);
            command.Parameters.Should().Contain("@p1", existingTodoItem.Id);
            command.Parameters.Should().Contain("@p2", existingTag.Id);
        });

        store.SqlCommands[4].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."CreatedAt", t1."Description", t1."DurationInHours", t1."LastModifiedAt", t1."Priority"
                FROM "TodoItems" AS t1
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", existingTodoItem.Id);
        });
    }
}
