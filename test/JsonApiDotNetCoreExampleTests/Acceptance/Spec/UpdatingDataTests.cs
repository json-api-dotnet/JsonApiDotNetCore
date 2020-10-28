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
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    // TODO: Move left-over tests in this file.

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
