using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    public sealed class UpdatingRelationshipsTests : IClassFixture<IntegrationTestContext<Startup, AppDbContext>>
    {
        private readonly IntegrationTestContext<Startup, AppDbContext> _testContext;

        private readonly Faker<TodoItem> _todoItemFaker = new Faker<TodoItem>()
            .RuleFor(t => t.Description, f => f.Lorem.Sentence())
            .RuleFor(t => t.Ordinal, f => f.Random.Number())
            .RuleFor(t => t.CreatedDate, f => f.Date.Past());

        private readonly Faker<Person> _personFaker = new Faker<Person>()
            .RuleFor(p => p.FirstName, f => f.Name.FirstName())
            .RuleFor(p => p.LastName, f => f.Name.LastName());

        public UpdatingRelationshipsTests(IntegrationTestContext<Startup, AppDbContext> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Can_Update_Cyclic_ToMany_Relationship_By_Patching_Resource()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();
            var otherTodoItem = _todoItemFaker.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.TodoItems.AddRange(todoItem, otherTodoItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "todoItems",
                    id = todoItem.StringId,
                    relationships = new Dictionary<string, object>
                    {
                        ["childrenTodos"] = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "todoItems",
                                    id = todoItem.StringId
                                },
                                new
                                {
                                    type = "todoItems",
                                    id = otherTodoItem.StringId
                                }
                            }
                        }
                    }
                }
            };

            var route = "/api/v1/todoItems/" + todoItem.StringId;

            // Act
            var (httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var todoItemInDatabase = await dbContext.TodoItems
                    .Include(item => item.ChildrenTodos)
                    .FirstAsync(item => item.Id == todoItem.Id);

                todoItemInDatabase.ChildrenTodos.Should().HaveCount(2);
                todoItemInDatabase.ChildrenTodos.Should().ContainSingle(x => x.Id == todoItem.Id);
                todoItemInDatabase.ChildrenTodos.Should().ContainSingle(x => x.Id == otherTodoItem.Id);
            });
        }

        [Fact]
        public async Task Can_Update_Cyclic_ToOne_Relationship_By_Patching_Resource()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.TodoItems.Add(todoItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "todoItems",
                    id = todoItem.StringId,
                    relationships = new Dictionary<string, object>
                    {
                        ["dependentOnTodo"] = new
                        {
                            data = new
                            {
                                type = "todoItems",
                                id = todoItem.StringId
                            }
                        }
                    }
                }
            };

            var route = "/api/v1/todoItems/" + todoItem.StringId;

            // Act
            var (httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var todoItemInDatabase = await dbContext.TodoItems
                    .Include(item => item.DependentOnTodo)
                    .FirstAsync(item => item.Id == todoItem.Id);

                todoItemInDatabase.DependentOnTodoId.Should().Be(todoItem.Id);
            });
        }

        [Fact]
        public async Task Can_Update_Both_Cyclic_ToOne_And_ToMany_Relationship_By_Patching_Resource()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();
            var otherTodoItem = _todoItemFaker.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.TodoItems.AddRange(todoItem, otherTodoItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "todoItems",
                    id = todoItem.StringId,
                    relationships = new Dictionary<string, object>
                    {
                        ["dependentOnTodo"] = new
                        {
                            data = new
                            {
                                type = "todoItems",
                                id = todoItem.StringId
                            }
                        },
                        ["childrenTodos"] = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "todoItems",
                                    id = todoItem.StringId
                                },
                                new
                                {
                                    type = "todoItems",
                                    id = otherTodoItem.StringId
                                }
                            }
                        }
                    }
                }
            };

            var route = "/api/v1/todoItems/" + todoItem.StringId;

            // Act
            var (httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var todoItemInDatabase = await dbContext.TodoItems
                    .Include(item => item.ParentTodo)
                    .FirstAsync(item => item.Id == todoItem.Id);

                todoItemInDatabase.ParentTodoId.Should().Be(todoItem.Id);
            });
        }
    }
}
