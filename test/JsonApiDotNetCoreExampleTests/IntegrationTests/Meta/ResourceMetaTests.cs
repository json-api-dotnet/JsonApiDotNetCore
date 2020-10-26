using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Meta
{
    public sealed class ResourceMetaTests : IClassFixture<IntegrationTestContext<Startup, AppDbContext>>
    {
        private readonly IntegrationTestContext<Startup, AppDbContext> _testContext;

        public ResourceMetaTests(IntegrationTestContext<Startup, AppDbContext> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task ResourceDefinition_That_Implements_GetMeta_Contains_Resource_Meta()
        {
            // Arrange
            var todoItems = new[]
            {
                new TodoItem {Id = 1, Description = "Important: Pay the bills"},
                new TodoItem {Id = 2, Description = "Plan my birthday party"},
                new TodoItem {Id = 3, Description = "Important: Call mom"}
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<TodoItem>();
                dbContext.TodoItems.AddRange(todoItems);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/todoItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(3);
            responseDocument.ManyData[0].Meta.Should().ContainKey("hasHighPriority");
            responseDocument.ManyData[1].Meta.Should().BeNull();
            responseDocument.ManyData[2].Meta.Should().ContainKey("hasHighPriority");
        }

        [Fact]
        public async Task ResourceDefinition_That_Implements_GetMeta_Contains_Include_Meta()
        {
            // Arrange
            var person = new Person
            {
                TodoItems = new HashSet<TodoItem>
                {
                    new TodoItem {Id = 1, Description = "Important: Pay the bills"}
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<TodoItem>();
                dbContext.People.Add(person);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/people/{person.StringId}?include=todoItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Meta.Should().ContainKey("hasHighPriority");
        }
    }
}
