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

public sealed class CreateResourceTests : IClassFixture<DapperTestContext>
{
    private readonly DapperTestContext _testContext;
    private readonly TestFakers _fakers = new();

    public CreateResourceTests(DapperTestContext testContext, ITestOutputHelper testOutputHelper)
    {
        testContext.SetTestOutputHelper(testOutputHelper);
        _testContext = testContext;
    }

    [Fact]
    public async Task Can_create_resource()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        TodoItem newTodoItem = _fakers.TodoItem.GenerateOne();

        Person existingPerson = _fakers.Person.GenerateOne();
        Tag existingTag = _fakers.Tag.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingPerson, existingTag);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "todoItems",
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
                            id = existingPerson.StringId
                        }
                    },
                    assignee = new
                    {
                        data = new
                        {
                            type = "people",
                            id = existingPerson.StringId
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

        const string route = "/todoItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("todoItems");
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("description").With(value => value.Should().Be(newTodoItem.Description));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("priority").With(value => value.Should().Be(newTodoItem.Priority));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("durationInHours").With(value => value.Should().Be(newTodoItem.DurationInHours));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("createdAt").With(value => value.Should().Be(DapperTestContext.FrozenTime));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("modifiedAt").With(value => value.Should().BeNull());

        responseDocument.Data.SingleValue.Relationships.ShouldOnlyContainKeys("owner", "assignee", "tags");

        long newTodoItemId = long.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());
        httpResponse.Headers.Location.Should().Be($"/todoItems/{newTodoItemId}");

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
            todoItemInDatabase.LastModifiedAt.Should().BeNull();

            todoItemInDatabase.Owner.ShouldNotBeNull();
            todoItemInDatabase.Owner.Id.Should().Be(existingPerson.Id);
            todoItemInDatabase.Assignee.ShouldNotBeNull();
            todoItemInDatabase.Assignee.Id.Should().Be(existingPerson.Id);
            todoItemInDatabase.Tags.ShouldHaveCount(1);
            todoItemInDatabase.Tags.ElementAt(0).Id.Should().Be(existingTag.Id);
        });

        store.SqlCommands.ShouldHaveCount(3);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                INSERT INTO "TodoItems" ("Description", "Priority", "DurationInHours", "CreatedAt", "LastModifiedAt", "OwnerId", "AssigneeId")
                VALUES (@p1, @p2, @p3, @p4, @p5, @p6, @p7)
                RETURNING "Id"
                """));

            command.Parameters.ShouldHaveCount(7);
            command.Parameters.Should().Contain("@p1", newTodoItem.Description);
            command.Parameters.Should().Contain("@p2", newTodoItem.Priority);
            command.Parameters.Should().Contain("@p3", newTodoItem.DurationInHours);
            command.Parameters.Should().Contain("@p4", DapperTestContext.FrozenTime);
            command.Parameters.Should().Contain("@p5", null);
            command.Parameters.Should().Contain("@p6", existingPerson.Id);
            command.Parameters.Should().Contain("@p7", existingPerson.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "Tags"
                SET "TodoItemId" = @p1
                WHERE "Id" = @p2
                """));

            command.Parameters.ShouldHaveCount(2);
            command.Parameters.Should().Contain("@p1", newTodoItemId);
            command.Parameters.Should().Contain("@p2", existingTag.Id);
        });

        store.SqlCommands[2].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."CreatedAt", t1."Description", t1."DurationInHours", t1."LastModifiedAt", t1."Priority"
                FROM "TodoItems" AS t1
                WHERE t1."Id" = @p1
                """));

            command.Parameters.ShouldHaveCount(1);
            command.Parameters.Should().Contain("@p1", newTodoItemId);
        });
    }

    [Fact]
    public async Task Can_create_resource_with_only_required_fields()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        TodoItem newTodoItem = _fakers.TodoItem.GenerateOne();

        Person existingPerson = _fakers.Person.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.People.Add(existingPerson);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "todoItems",
                attributes = new
                {
                    description = newTodoItem.Description,
                    priority = newTodoItem.Priority
                },
                relationships = new
                {
                    owner = new
                    {
                        data = new
                        {
                            type = "people",
                            id = existingPerson.StringId
                        }
                    }
                }
            }
        };

        const string route = "/todoItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("todoItems");
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("description").With(value => value.Should().Be(newTodoItem.Description));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("priority").With(value => value.Should().Be(newTodoItem.Priority));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("durationInHours").With(value => value.Should().BeNull());
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("createdAt").With(value => value.Should().Be(DapperTestContext.FrozenTime));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("modifiedAt").With(value => value.Should().BeNull());
        responseDocument.Data.SingleValue.Relationships.ShouldOnlyContainKeys("owner", "assignee", "tags");

        long newTodoItemId = long.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

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
            todoItemInDatabase.DurationInHours.Should().BeNull();
            todoItemInDatabase.CreatedAt.Should().Be(DapperTestContext.FrozenTime);
            todoItemInDatabase.LastModifiedAt.Should().BeNull();

            todoItemInDatabase.Owner.ShouldNotBeNull();
            todoItemInDatabase.Owner.Id.Should().Be(existingPerson.Id);
            todoItemInDatabase.Assignee.Should().BeNull();
            todoItemInDatabase.Tags.Should().BeEmpty();
        });

        store.SqlCommands.ShouldHaveCount(2);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                INSERT INTO "TodoItems" ("Description", "Priority", "DurationInHours", "CreatedAt", "LastModifiedAt", "OwnerId", "AssigneeId")
                VALUES (@p1, @p2, @p3, @p4, @p5, @p6, @p7)
                RETURNING "Id"
                """));

            command.Parameters.ShouldHaveCount(7);
            command.Parameters.Should().Contain("@p1", newTodoItem.Description);
            command.Parameters.Should().Contain("@p2", newTodoItem.Priority);
            command.Parameters.Should().Contain("@p3", null);
            command.Parameters.Should().Contain("@p4", DapperTestContext.FrozenTime);
            command.Parameters.Should().Contain("@p5", null);
            command.Parameters.Should().Contain("@p6", existingPerson.Id);
            command.Parameters.Should().Contain("@p7", null);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."CreatedAt", t1."Description", t1."DurationInHours", t1."LastModifiedAt", t1."Priority"
                FROM "TodoItems" AS t1
                WHERE t1."Id" = @p1
                """));

            command.Parameters.ShouldHaveCount(1);
            command.Parameters.Should().Contain("@p1", newTodoItemId);
        });
    }

    [Fact]
    public async Task Cannot_create_resource_without_required_fields()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        var requestBody = new
        {
            data = new
            {
                type = "todoItems",
                attributes = new
                {
                }
            }
        };

        const string route = "/todoItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(3);

        ErrorObject error1 = responseDocument.Errors[0];
        error1.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error1.Title.Should().Be("Input validation failed.");
        error1.Detail.Should().Be("The Owner field is required.");
        error1.Source.ShouldNotBeNull();
        error1.Source.Pointer.Should().Be("/data/relationships/owner/data");

        ErrorObject error2 = responseDocument.Errors[1];
        error2.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error2.Title.Should().Be("Input validation failed.");
        error2.Detail.Should().Be("The Priority field is required.");
        error2.Source.ShouldNotBeNull();
        error2.Source.Pointer.Should().Be("/data/attributes/priority");

        ErrorObject error3 = responseDocument.Errors[2];
        error3.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error3.Title.Should().Be("Input validation failed.");
        error3.Detail.Should().Be("The Description field is required.");
        error3.Source.ShouldNotBeNull();
        error3.Source.Pointer.Should().Be("/data/attributes/description");

        store.SqlCommands.Should().BeEmpty();
    }

    [Fact]
    public async Task Can_create_resource_with_unmapped_property()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        AccountRecovery existingAccountRecovery = _fakers.AccountRecovery.GenerateOne();
        Person existingPerson = _fakers.Person.GenerateOne();

        string newUserName = _fakers.LoginAccount.GenerateOne().UserName;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingAccountRecovery, existingPerson);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "loginAccounts",
                attributes = new
                {
                    userName = newUserName
                },
                relationships = new
                {
                    recovery = new
                    {
                        data = new
                        {
                            type = "accountRecoveries",
                            id = existingAccountRecovery.StringId
                        }
                    },
                    person = new
                    {
                        data = new
                        {
                            type = "people",
                            id = existingPerson.StringId
                        }
                    }
                }
            }
        };

        const string route = "/loginAccounts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("loginAccounts");
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("userName").With(value => value.Should().Be(newUserName));
        responseDocument.Data.SingleValue.Attributes.Should().NotContainKey("lastUsedAt");
        responseDocument.Data.SingleValue.Relationships.ShouldOnlyContainKeys("recovery", "person");

        long newLoginAccountId = long.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            LoginAccount loginAccountInDatabase = await dbContext.LoginAccounts
                .Include(todoItem => todoItem.Recovery)
                .Include(todoItem => todoItem.Person)
                .FirstWithIdAsync(newLoginAccountId);

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            loginAccountInDatabase.UserName.Should().Be(newUserName);
            loginAccountInDatabase.LastUsedAt.Should().BeNull();

            loginAccountInDatabase.Recovery.ShouldNotBeNull();
            loginAccountInDatabase.Recovery.Id.Should().Be(existingAccountRecovery.Id);
            loginAccountInDatabase.Person.ShouldNotBeNull();
            loginAccountInDatabase.Person.Id.Should().Be(existingPerson.Id);
        });

        store.SqlCommands.ShouldHaveCount(4);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                DELETE FROM "LoginAccounts"
                WHERE "RecoveryId" = @p1
                """));

            command.Parameters.ShouldHaveCount(1);
            command.Parameters.Should().Contain("@p1", existingAccountRecovery.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                INSERT INTO "LoginAccounts" ("UserName", "LastUsedAt", "RecoveryId")
                VALUES (@p1, @p2, @p3)
                RETURNING "Id"
                """));

            command.Parameters.ShouldHaveCount(3);
            command.Parameters.Should().Contain("@p1", newUserName);
            command.Parameters.Should().Contain("@p2", null);
            command.Parameters.Should().Contain("@p3", existingAccountRecovery.Id);
        });

        store.SqlCommands[2].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                UPDATE "People"
                SET "AccountId" = @p1
                WHERE "Id" = @p2
                """));

            command.Parameters.ShouldHaveCount(2);
            command.Parameters.Should().Contain("@p1", newLoginAccountId);
            command.Parameters.Should().Contain("@p2", existingPerson.Id);
        });

        store.SqlCommands[3].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id", t1."LastUsedAt", t1."UserName"
                FROM "LoginAccounts" AS t1
                WHERE t1."Id" = @p1
                """));

            command.Parameters.ShouldHaveCount(1);
            command.Parameters.Should().Contain("@p1", newLoginAccountId);
        });
    }

    [Fact]
    public async Task Can_create_resource_with_calculated_attribute()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Person newPerson = _fakers.Person.GenerateOne();

        var requestBody = new
        {
            data = new
            {
                type = "people",
                attributes = new
                {
                    firstName = newPerson.FirstName,
                    lastName = newPerson.LastName
                }
            }
        };

        const string route = "/people";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("people");
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("firstName").With(value => value.Should().Be(newPerson.FirstName));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("lastName").With(value => value.Should().Be(newPerson.LastName));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("displayName").With(value => value.Should().Be(newPerson.DisplayName));
        responseDocument.Data.SingleValue.Relationships.ShouldOnlyContainKeys("account", "ownedTodoItems", "assignedTodoItems");

        long newPersonId = long.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Person personInDatabase = await dbContext.People.FirstWithIdAsync(newPersonId);

            personInDatabase.FirstName.Should().Be(newPerson.FirstName);
            personInDatabase.LastName.Should().Be(newPerson.LastName);
            personInDatabase.DisplayName.Should().Be(newPerson.DisplayName);
        });

        store.SqlCommands.ShouldHaveCount(2);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                INSERT INTO "People" ("FirstName", "LastName", "AccountId")
                VALUES (@p1, @p2, @p3)
                RETURNING "Id"
                """));

            command.Parameters.ShouldHaveCount(3);
            command.Parameters.Should().Contain("@p1", newPerson.FirstName);
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

            command.Parameters.ShouldHaveCount(1);
            command.Parameters.Should().Contain("@p1", newPersonId);
        });
    }

    [Fact]
    public async Task Can_create_resource_with_client_generated_ID()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        Tag existingTag = _fakers.Tag.GenerateOne();

        RgbColor newColor = _fakers.RgbColor.GenerateOne();

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
                type = "rgbColors",
                id = newColor.StringId,
                relationships = new
                {
                    tag = new
                    {
                        data = new
                        {
                            type = "tags",
                            id = existingTag.StringId
                        }
                    }
                }
            }
        };

        const string route = "/rgbColors/";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            RgbColor colorInDatabase = await dbContext.RgbColors.Include(rgbColor => rgbColor.Tag).FirstWithIdAsync(newColor.Id);

            colorInDatabase.Red.Should().Be(newColor.Red);
            colorInDatabase.Green.Should().Be(newColor.Green);
            colorInDatabase.Blue.Should().Be(newColor.Blue);

            colorInDatabase.Tag.ShouldNotBeNull();
            colorInDatabase.Tag.Id.Should().Be(existingTag.Id);
        });

        store.SqlCommands.ShouldHaveCount(3);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                DELETE FROM "RgbColors"
                WHERE "TagId" = @p1
                """));

            command.Parameters.ShouldHaveCount(1);
            command.Parameters.Should().Contain("@p1", existingTag.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                INSERT INTO "RgbColors" ("Id", "TagId")
                VALUES (@p1, @p2)
                RETURNING "Id"
                """, true));

            command.Parameters.ShouldHaveCount(2);
            command.Parameters.Should().Contain("@p1", newColor.Id);
            command.Parameters.Should().Contain("@p2", existingTag.Id);
        });

        store.SqlCommands[2].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id"
                FROM "RgbColors" AS t1
                WHERE t1."Id" = @p1
                """));

            command.Parameters.ShouldHaveCount(1);
            command.Parameters.Should().Contain("@p1", newColor.Id);
        });
    }

    [Fact]
    public async Task Cannot_create_resource_for_existing_client_generated_ID()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<SqlCaptureStore>();
        store.Clear();

        RgbColor existingColor = _fakers.RgbColor.GenerateOne();
        existingColor.Tag = _fakers.Tag.GenerateOne();

        Tag existingTag = _fakers.Tag.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await _testContext.ClearAllTablesAsync(dbContext);
            dbContext.AddInRange(existingColor, existingTag);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "rgbColors",
                id = existingColor.StringId,
                relationships = new
                {
                    tag = new
                    {
                        data = new
                        {
                            type = "tags",
                            id = existingTag.StringId
                        }
                    }
                }
            }
        };

        const string route = "/rgbColors";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().Be("Another resource with the specified ID already exists.");
        error.Detail.Should().Be($"Another resource of type 'rgbColors' with ID '{existingColor.StringId}' already exists.");
        error.Source.Should().BeNull();

        store.SqlCommands.ShouldHaveCount(3);

        store.SqlCommands[0].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                DELETE FROM "RgbColors"
                WHERE "TagId" = @p1
                """));

            command.Parameters.ShouldHaveCount(1);
            command.Parameters.Should().Contain("@p1", existingTag.Id);
        });

        store.SqlCommands[1].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                INSERT INTO "RgbColors" ("Id", "TagId")
                VALUES (@p1, @p2)
                RETURNING "Id"
                """, true));

            command.Parameters.ShouldHaveCount(2);
            command.Parameters.Should().Contain("@p1", existingColor.Id);
            command.Parameters.Should().Contain("@p2", existingTag.Id);
        });

        store.SqlCommands[2].With(command =>
        {
            command.Statement.Should().Be(_testContext.AdaptSql("""
                SELECT t1."Id"
                FROM "RgbColors" AS t1
                WHERE t1."Id" = @p1
                """));

            command.Parameters.ShouldHaveCount(1);
            command.Parameters.Should().Contain("@p1", existingColor.Id);
        });
    }
}
