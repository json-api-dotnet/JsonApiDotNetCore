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

namespace DapperTests.IntegrationTests.AtomicOperations;

public sealed class AtomicOperationsTests : IClassFixture<DapperTestContext>
{
    private readonly DapperTestContext _testContext;
    private readonly TestFakers _fakers = new();

    public AtomicOperationsTests(DapperTestContext testContext, ITestOutputHelper testOutputHelper)
    {
        testContext.SetTestOutputHelper(testOutputHelper);
        _testContext = testContext;
    }

    [Fact]
    public async Task Can_use_multiple_operations()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person newOwner = _fakers.Person.GenerateOne();
        Person newAssignee = _fakers.Person.GenerateOne();
        Tag newTag = _fakers.Tag.GenerateOne();
        TodoItem newTodoItem = _fakers.TodoItem.GenerateOne();

        const string ownerLocalId = "new-owner";
        const string assigneeLocalId = "new-assignee";
        const string tagLocalId = "new-tag";
        const string todoItemLocalId = "new-todoItem";

        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "people",
                        lid = ownerLocalId,
                        attributes = new
                        {
                            firstName = newOwner.FirstName,
                            lastName = newOwner.LastName
                        }
                    }
                },
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "people",
                        lid = assigneeLocalId,
                        attributes = new
                        {
                            firstName = newAssignee.FirstName,
                            lastName = newAssignee.LastName
                        }
                    }
                },
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "tags",
                        lid = tagLocalId,
                        attributes = new
                        {
                            name = newTag.Name
                        }
                    }
                },
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "todoItems",
                        lid = todoItemLocalId,
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
                                    lid = ownerLocalId
                                }
                            }
                        }
                    }
                },
                new
                {
                    op = "update",
                    @ref = new
                    {
                        type = "todoItems",
                        lid = todoItemLocalId,
                        relationship = "assignee"
                    },
                    data = new
                    {
                        type = "people",
                        lid = assigneeLocalId
                    }
                },
                new
                {
                    op = "update",
                    data = new
                    {
                        type = "todoItems",
                        lid = todoItemLocalId,
                        relationships = new
                        {
                            tags = new
                            {
                                data = new[]
                                {
                                    new
                                    {
                                        type = "tags",
                                        lid = tagLocalId
                                    }
                                }
                            }
                        }
                    }
                },
                new
                {
                    op = "remove",
                    @ref = new
                    {
                        type = "people",
                        lid = assigneeLocalId
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Results.Should().HaveCount(7);

        responseDocument.Results[0].Data.SingleValue.RefShould().NotBeNull().And.Subject.Type.Should().Be("people");
        responseDocument.Results[1].Data.SingleValue.RefShould().NotBeNull().And.Subject.Type.Should().Be("people");
        responseDocument.Results[2].Data.SingleValue.RefShould().NotBeNull().And.Subject.Type.Should().Be("tags");
        responseDocument.Results[3].Data.SingleValue.RefShould().NotBeNull().And.Subject.Type.Should().Be("todoItems");
        responseDocument.Results[4].Data.Value.Should().BeNull();
        responseDocument.Results[5].Data.SingleValue.RefShould().NotBeNull().And.Subject.Type.Should().Be("todoItems");
        responseDocument.Results[6].Data.Value.Should().BeNull();

        long newOwnerId = long.Parse(responseDocument.Results[0].Data.SingleValue!.Id.Should().NotBeNull().And.Subject);
        long newAssigneeId = long.Parse(responseDocument.Results[1].Data.SingleValue!.Id.Should().NotBeNull().And.Subject);
        long newTagId = long.Parse(responseDocument.Results[2].Data.SingleValue!.Id.Should().NotBeNull().And.Subject);
        long newTodoItemId = long.Parse(responseDocument.Results[3].Data.SingleValue!.Id.Should().NotBeNull().And.Subject);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            TodoItem todoItemInDatabase = await dbContext.TodoItems
                .Include(todoItem => todoItem.Owner)
                .Include(todoItem => todoItem.Assignee)
                .Include(todoItem => todoItem.Tags)
                .FirstWithIdAsync(newTodoItemId);

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            todoItemInDatabase.Description.Should().Be(newTodoItem.Description);
            todoItemInDatabase.Priority.Should().Be(newTodoItem.Priority);
            todoItemInDatabase.DurationInHours.Should().Be(newTodoItem.DurationInHours);
            todoItemInDatabase.CreatedAt.Should().Be(DapperTestContext.FrozenTime);
            todoItemInDatabase.LastModifiedAt.Should().Be(DapperTestContext.FrozenTime);

            todoItemInDatabase.Owner.Should().NotBeNull();
            todoItemInDatabase.Owner.Id.Should().Be(newOwnerId);
            todoItemInDatabase.Assignee.Should().BeNull();
            todoItemInDatabase.Tags.Should().HaveCount(1);
            todoItemInDatabase.Tags.ElementAt(0).Id.Should().Be(newTagId);
        });

        store.SqlCommands.Should().HaveCount(15);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                INSERT INTO "People" ("FirstName", "LastName", "AccountId")
                VALUES (@p1, @p2, @p3)
                RETURNING "Id"
                """));

            command.Parameters.Should().HaveCount(3);
            command.Parameters.Should().Contain("@p1", newOwner.FirstName);
            command.Parameters.Should().Contain("@p2", newOwner.LastName);
            command.Parameters.Should().Contain("@p3", null);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."FirstName", t1."LastName"
                FROM "People" AS t1
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", newOwnerId);
        });

        store.SqlCommands[2].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                INSERT INTO "People" ("FirstName", "LastName", "AccountId")
                VALUES (@p1, @p2, @p3)
                RETURNING "Id"
                """));

            command.Parameters.Should().HaveCount(3);
            command.Parameters.Should().Contain("@p1", newAssignee.FirstName);
            command.Parameters.Should().Contain("@p2", newAssignee.LastName);
            command.Parameters.Should().Contain("@p3", null);
        });

        store.SqlCommands[3].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."FirstName", t1."LastName"
                FROM "People" AS t1
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", newAssigneeId);
        });

        store.SqlCommands[4].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                INSERT INTO "Tags" ("Name", "TodoItemId")
                VALUES (@p1, @p2)
                RETURNING "Id"
                """));

            command.Parameters.Should().HaveCount(2);
            command.Parameters.Should().Contain("@p1", newTag.Name);
            command.Parameters.Should().Contain("@p2", null);
        });

        store.SqlCommands[5].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."Name"
                FROM "Tags" AS t1
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", newTagId);
        });

        store.SqlCommands[6].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                INSERT INTO "TodoItems" ("Description", "Priority", "DurationInHours", "CreatedAt", "LastModifiedAt", "OwnerId", "AssigneeId")
                VALUES (@p1, @p2, @p3, @p4, @p5, @p6, @p7)
                RETURNING "Id"
                """));

            command.Parameters.Should().HaveCount(7);
            command.Parameters.Should().Contain("@p1", newTodoItem.Description);
            command.Parameters.Should().Contain("@p2", newTodoItem.Priority);
            command.Parameters.Should().Contain("@p3", newTodoItem.DurationInHours);
            command.Parameters.Should().Contain("@p4", DapperTestContext.FrozenTime);
            command.Parameters.Should().Contain("@p5", null);
            command.Parameters.Should().Contain("@p6", newOwnerId);
            command.Parameters.Should().Contain("@p7", null);
        });

        store.SqlCommands[7].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."CreatedAt", t1."Description", t1."DurationInHours", t1."LastModifiedAt", t1."Priority"
                FROM "TodoItems" AS t1
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", newTodoItemId);
        });

        store.SqlCommands[8].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."CreatedAt", t1."Description", t1."DurationInHours", t1."LastModifiedAt", t1."Priority", t2."Id", t2."FirstName", t2."LastName"
                FROM "TodoItems" AS t1
                LEFT JOIN "People" AS t2 ON t1."AssigneeId" = t2."Id"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", newTodoItemId);
        });

        store.SqlCommands[9].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "TodoItems"
                SET "AssigneeId" = @p1
                WHERE "Id" = @p2
                """));

            command.Parameters.Should().HaveCount(2);
            command.Parameters.Should().Contain("@p1", newAssigneeId);
            command.Parameters.Should().Contain("@p2", newTodoItemId);
        });

        store.SqlCommands[10].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."CreatedAt", t1."Description", t1."DurationInHours", t1."LastModifiedAt", t1."Priority", t2."Id", t2."Name"
                FROM "TodoItems" AS t1
                LEFT JOIN "Tags" AS t2 ON t1."Id" = t2."TodoItemId"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", newTodoItemId);
        });

        store.SqlCommands[11].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "TodoItems"
                SET "LastModifiedAt" = @p1
                WHERE "Id" = @p2
                """));

            command.Parameters.Should().HaveCount(2);
            command.Parameters.Should().Contain("@p1", DapperTestContext.FrozenTime);
            command.Parameters.Should().Contain("@p2", newTodoItemId);
        });

        store.SqlCommands[12].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "Tags"
                SET "TodoItemId" = @p1
                WHERE "Id" = @p2
                """));

            command.Parameters.Should().HaveCount(2);
            command.Parameters.Should().Contain("@p1", newTodoItemId);
            command.Parameters.Should().Contain("@p2", newTagId);
        });

        store.SqlCommands[13].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."CreatedAt", t1."Description", t1."DurationInHours", t1."LastModifiedAt", t1."Priority"
                FROM "TodoItems" AS t1
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", newTodoItemId);
        });

        store.SqlCommands[14].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                DELETE FROM "People"
                WHERE "Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().Contain("@p1", newAssigneeId);
        });
    }

    [Fact]
    public async Task Can_rollback_on_error()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person newPerson = _fakers.Person.GenerateOne();

        const long unknownTodoItemId = Unknown.TypedId.Int64;

        const string personLocalId = "new-person";

        await _testContext.RunOnDatabaseAsync(_testContext.ClearAllTablesAsync);

        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "people",
                        lid = personLocalId,
                        attributes = new
                        {
                            lastName = newPerson.LastName
                        }
                    }
                },
                new
                {
                    op = "update",
                    @ref = new
                    {
                        type = "people",
                        lid = personLocalId,
                        relationship = "assignedTodoItems"
                    },
                    data = new[]
                    {
                        new
                        {
                            type = "todoItems",
                            id = unknownTodoItemId.ToString()
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'todoItems' with ID '{unknownTodoItemId}' in relationship 'assignedTodoItems' does not exist.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[1]");

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<Person> peopleInDatabase = await dbContext.People.ToListAsync();
            peopleInDatabase.Should().BeEmpty();
        });

        store.SqlCommands.Should().HaveCount(5);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                INSERT INTO "People" ("FirstName", "LastName", "AccountId")
                VALUES (@p1, @p2, @p3)
                RETURNING "Id"
                """));

            command.Parameters.Should().HaveCount(3);
            command.Parameters.Should().Contain("@p1", null);
            command.Parameters.Should().Contain("@p2", newPerson.LastName);
            command.Parameters.Should().Contain("@p3", null);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."FirstName", t1."LastName"
                FROM "People" AS t1
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().ContainKey("@p1").WhoseValue.Should().NotBeNull();
        });

        store.SqlCommands[2].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."FirstName", t1."LastName", t2."Id", t2."CreatedAt", t2."Description", t2."DurationInHours", t2."LastModifiedAt", t2."Priority"
                FROM "People" AS t1
                LEFT JOIN "TodoItems" AS t2 ON t1."Id" = t2."AssigneeId"
                WHERE t1."Id" = @p1
                """));

            command.Parameters.Should().HaveCount(1);
            command.Parameters.Should().ContainKey("@p1").WhoseValue.Should().NotBeNull();
        });

        store.SqlCommands[3].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "TodoItems"
                SET "AssigneeId" = @p1
                WHERE "Id" = @p2
                """));

            command.Parameters.Should().HaveCount(2);
            command.Parameters.Should().ContainKey("@p1").WhoseValue.Should().NotBeNull();
            command.Parameters.Should().Contain("@p2", unknownTodoItemId);
        });

        store.SqlCommands[4].With(command =>
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
