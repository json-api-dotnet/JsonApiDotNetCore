using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Meta
{
    public sealed class ResourceMetaTests : IClassFixture<ExampleIntegrationTestContext<Startup, AppDbContext>>
    {
        private readonly ExampleIntegrationTestContext<Startup, AppDbContext> _testContext;

        public ResourceMetaTests(ExampleIntegrationTestContext<Startup, AppDbContext> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Returns_resource_meta_from_ResourceDefinition()
        {
            // Arrange
            var todoItems = new[]
            {
                new TodoItem {Description = "Important: Pay the bills"},
                new TodoItem {Description = "Plan my birthday party"},
                new TodoItem {Description = "Important: Call mom"}
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
        public async Task Returns_resource_meta_from_ResourceDefinition_in_included_resources()
        {
            // Arrange
            var person = new Person
            {
                TodoItems = new HashSet<TodoItem>
                {
                    new TodoItem {Description = "Important: Pay the bills"}
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
