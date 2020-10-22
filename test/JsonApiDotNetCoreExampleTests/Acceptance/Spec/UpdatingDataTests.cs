using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    public sealed class UpdatingDataTests : IClassFixture<IntegrationTestContext<Startup, AppDbContext>>
    {
        private readonly IntegrationTestContext<Startup, AppDbContext> _testContext;

        private readonly Faker<TodoItem> _todoItemFaker = new Faker<TodoItem>()
            .RuleFor(t => t.Description, f => f.Lorem.Sentence())
            .RuleFor(t => t.Ordinal, f => f.Random.Number())
            .RuleFor(t => t.CreatedDate, f => f.Date.Past());

        private readonly Faker<Person> _personFaker = new Faker<Person>()
            .RuleFor(p => p.FirstName, f => f.Name.FirstName())
            .RuleFor(p => p.LastName, f => f.Name.LastName());

        public UpdatingDataTests(IntegrationTestContext<Startup, AppDbContext> testContext)
        {
            _testContext = testContext;

            FakeLoggerFactory loggerFactory = null;

            testContext.ConfigureLogging(options =>
            {
                loggerFactory = new FakeLoggerFactory();

                options.ClearProviders();
                options.AddProvider(loggerFactory);
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddFilter((category, level) => level == LogLevel.Trace &&
                    (category == typeof(JsonApiReader).FullName || category == typeof(JsonApiWriter).FullName));
            });

            testContext.ConfigureServicesBeforeStartup(services =>
            {
                if (loggerFactory != null)
                {
                    services.AddSingleton(_ => loggerFactory);
                }
            });
        }

        [Fact]
        public async Task PatchResource_ModelWithEntityFrameworkInheritance_IsPatched()
        {
            // Arrange
            var clock = _testContext.Factory.Services.GetRequiredService<ISystemClock>();

            SuperUser superUser = null;
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                superUser = new SuperUser(dbContext)
                {
                    SecurityLevel = 1337,
                    UserName = "joe@account.com",
                    Password = "12345",
                    LastPasswordChange = clock.UtcNow.LocalDateTime.AddMinutes(-15)
                };

                dbContext.Users.Add(superUser);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "superUsers",
                    id = superUser.StringId,
                    attributes = new Dictionary<string, object>
                    {
                        ["securityLevel"] = 2674,
                        ["userName"] = "joe@other-domain.com",
                        ["password"] = "secret"
                    }
                }
            };

            var route = "/api/v1/superUsers/" + superUser.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Attributes["securityLevel"].Should().Be(2674);
            responseDocument.SingleData.Attributes["userName"].Should().Be("joe@other-domain.com");
            responseDocument.SingleData.Attributes.Should().NotContainKey("password");
        }

        [Fact]
        public async Task Response_422_If_Updating_Not_Settable_Attribute()
        {
            // Arrange
            var loggerFactory = _testContext.Factory.Services.GetRequiredService<FakeLoggerFactory>();
            loggerFactory.Logger.Clear();

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
                    attributes = new Dictionary<string, object>
                    {
                        ["calculatedValue"] = "calculated"
                    }
                }
            };

            var route = "/api/v1/todoItems/" + todoItem.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body.");
            responseDocument.Errors[0].Detail.Should().StartWith("Property 'TodoItem.CalculatedValue' is read-only. - Request body: <<");

            loggerFactory.Logger.Messages.Should().HaveCount(2);
            loggerFactory.Logger.Messages.Should().Contain(x =>
                x.Text.StartsWith("Received request at ") && x.Text.Contains("with body:"));
            loggerFactory.Logger.Messages.Should().Contain(x =>
                x.Text.StartsWith("Sending 422 response for request at ") &&
                x.Text.Contains("Failed to deserialize request body."));
        }

        [Fact]
        public async Task Respond_404_If_ResourceDoesNotExist()
        {
            // Arrange
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<TodoItem>();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "todoItems",
                    id = 99999999,
                    attributes = new Dictionary<string, object>
                    {
                        ["description"] = "something else"
                    }
                }
            };

            var route = "/api/v1/todoItems/" + 99999999;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("The requested resource does not exist.");
            responseDocument.Errors[0].Detail.Should().Be("Resource of type 'todoItems' with ID '99999999' does not exist.");
        }

        [Fact]
        public async Task Respond_422_If_IdNotInAttributeList()
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
                    attributes = new Dictionary<string, object>
                    {
                        ["description"] = "something else"
                    }
                }
            };

            var route = "/api/v1/todoItems/" + todoItem.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Payload must include 'id' element.");
            responseDocument.Errors[0].Detail.Should().StartWith("Request body: <<");
        }

        [Fact]
        public async Task Respond_409_If_IdInUrlIsDifferentFromIdInRequestBody()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.TodoItems.Add(todoItem);
                await dbContext.SaveChangesAsync();
            });

            int differentTodoItemId = todoItem.Id + 1;

            var requestBody = new
            {
                data = new
                {
                    type = "todoItems",
                    id = differentTodoItemId,
                    attributes = new Dictionary<string, object>
                    {
                        ["description"] = "something else"
                    }
                }
            };

            var route = "/api/v1/todoItems/" + todoItem.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Conflict);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Conflict);
            responseDocument.Errors[0].Title.Should().Be("Resource ID mismatch between request body and endpoint URL.");
            responseDocument.Errors[0].Detail.Should().Be($"Expected resource ID '{todoItem.Id}' in PATCH request body at endpoint 'http://localhost/api/v1/todoItems/{todoItem.Id}', instead of '{differentTodoItemId}'.");
        }

        [Fact]
        public async Task Respond_422_If_Broken_JSON_Payload()
        {
            // Arrange
            var requestBody = "{ \"data\" {";

            var route = "/api/v1/todoItems/";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body.");
            responseDocument.Errors[0].Detail.Should().StartWith("Invalid character after parsing");
        }

        [Fact]
        public async Task Respond_422_If_Blocked_For_Update()
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
                    attributes = new Dictionary<string, object>
                    {
                        ["offsetDate"] = "2000-01-01"
                    }
                }
            };

            var route = "/api/v1/todoItems/" + todoItem.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Changing the value of the requested attribute is not allowed.");
            responseDocument.Errors[0].Detail.Should().StartWith("Changing the value of 'offsetDate' is not allowed. - Request body:");
        }

        [Fact]
        public async Task Can_Patch_Resource()
        {
            // Arrange
            var person = _personFaker.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.People.Add(person);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "people",
                    id = person.StringId,
                    attributes = new Dictionary<string, object>
                    {
                        ["lastName"] = "Johnson",
                    }
                }
            };

            var route = "/api/v1/people/" + person.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var updated = await dbContext.People
                    .FirstAsync(t => t.Id == person.Id);

                updated.LastName.Should().Be("Johnson");
            });
        }
        
        [Fact]
        public async Task Can_Patch_Resource_And_Get_Response_With_Side_Effects()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = _personFaker.Generate();
            var currentStateOfAlwaysChangingValue = todoItem.AlwaysChangingValue;

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
                    attributes = new Dictionary<string, object>
                    {
                        ["description"] = "something else",
                        ["ordinal"] = 1
                    }
                }
            };

            var route = "/api/v1/todoItems/" + todoItem.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Attributes["description"].Should().Be("something else");
            responseDocument.SingleData.Attributes["ordinal"].Should().Be(1);
            responseDocument.SingleData.Attributes["alwaysChangingValue"].Should().NotBe(currentStateOfAlwaysChangingValue);
            responseDocument.SingleData.Relationships["owner"].SingleData.Should().BeNull();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var updated = await dbContext.TodoItems
                    .Include(t => t.Owner)
                    .FirstAsync(t => t.Id == todoItem.Id);

                updated.Description.Should().Be("something else");
                updated.Ordinal.Should().Be(1);
                updated.AlwaysChangingValue.Should().NotBe(currentStateOfAlwaysChangingValue);
                updated.Owner.Id.Should().Be(todoItem.Owner.Id);
            });
        }

        [Fact]
        public async Task Can_Patch_Resource_With_Side_Effects_And_Apply_Sparse_Field_Set_Selection()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = _personFaker.Generate();

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
                    attributes = new Dictionary<string, object>
                    {
                        ["description"] = "something else",
                        ["ordinal"] = 1
                    }
                }
            };

            var route = $"/api/v1/todoItems/{todoItem.StringId}?fields=description,ordinal";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Should().NotBeNull();
            responseDocument.SingleData.Attributes.Should().HaveCount(2);
            responseDocument.SingleData.Attributes["description"].Should().Be("something else");
            responseDocument.SingleData.Attributes["ordinal"].Should().Be(1);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var updated = await dbContext.TodoItems
                    .Include(t => t.Owner)
                    .FirstAsync(t => t.Id == todoItem.Id);

                updated.Description.Should().Be("something else");
                updated.Ordinal.Should().Be(1);
                updated.Owner.Id.Should().Be(todoItem.Owner.Id);
            });
        }

        // TODO: Add test(s) that save a relationship, then return its data via include.

        // TODO: Add test for DeleteRelationshipAsync that only deletes non-existing from the right resources in to-many relationship.

        // TODO: This test is flaky.
        [Fact]
        public async Task Patch_Resource_With_HasMany_Does_Not_Include_Relationships()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = _personFaker.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.TodoItems.Add(todoItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "people",
                    id = todoItem.Owner.StringId,
                    attributes = new Dictionary<string, object>
                    {
                        ["firstName"] = "John",
                        ["lastName"] = "Doe"
                    }
                }
            };

            var route = "/api/v1/people/" + todoItem.Owner.StringId;

            // Act
            //var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);
            var (httpResponse, responseText) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            try
            {
                // Assert
                httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

                var responseDocument = JsonConvert.DeserializeObject<Document>(responseText);

                responseDocument.SingleData.Should().NotBeNull();
                responseDocument.SingleData.Attributes["firstName"].Should().Be("John");
                responseDocument.SingleData.Attributes["lastName"].Should().Be("Doe");
                responseDocument.SingleData.Relationships.Should().ContainKey("todoItems");
                responseDocument.SingleData.Relationships["todoItems"].Data.Should().BeNull();
            }
            catch (Exception exception)
            {
                throw new Exception("Flaky test failed with response status " + (int)httpResponse.StatusCode + " and body: <<" + responseText + ">>", exception);
            }
        }

        [Fact]
        public async Task Can_Patch_Resource_And_HasOne_Relationships()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();
            var person = _personFaker.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.TodoItems.Add(todoItem);
                dbContext.People.Add(person);
                
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "todoItems",
                    id = todoItem.StringId,
                    attributes = new Dictionary<string, object>
                    {
                        ["description"] = "Something else"
                    },
                    relationships = new Dictionary<string, object>
                    {
                        ["owner"] = new
                        {
                            data = new
                            {
                                type = "people",
                                id = person.StringId
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
                var updated = await dbContext.TodoItems
                    .Include(t => t.Owner)
                    .FirstAsync(t => t.Id == todoItem.Id);

                updated.Description.Should().Be("Something else");
                updated.Owner.Id.Should().Be(person.Id);
            });
        }
    }
}
