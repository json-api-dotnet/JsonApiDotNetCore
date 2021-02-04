using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Meta
{
    public sealed class TopLevelCountTests : IClassFixture<ExampleIntegrationTestContext<Startup, AppDbContext>>
    {
        private readonly ExampleIntegrationTestContext<Startup, AppDbContext> _testContext;

        public TopLevelCountTests(ExampleIntegrationTestContext<Startup, AppDbContext> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.IncludeTotalResourceCount = true;
        }

        [Fact]
        public async Task Total_Resource_Count_Included_For_Collection()
        {
            // Arrange
            var todoItem = new TodoItem();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<TodoItem>();
                dbContext.TodoItems.Add(todoItem);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/todoItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Meta.Should().NotBeNull();
            responseDocument.Meta["totalResources"].Should().Be(1);
        }

        [Fact]
        public async Task Total_Resource_Count_Included_For_Empty_Collection()
        {
            // Arrange
            await _testContext.RunOnDatabaseAsync(async dbContext => { await dbContext.ClearTableAsync<TodoItem>(); });

            var route = "/api/v1/todoItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Meta.Should().NotBeNull();
            responseDocument.Meta["totalResources"].Should().Be(0);
        }

        [Fact]
        public async Task Total_Resource_Count_Excluded_From_POST_Response()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "todoItems",
                    attributes = new
                    {
                        description = "Something"
                    }
                }
            };

            var route = "/api/v1/todoItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.Meta.Should().BeNull();
        }

        [Fact]
        public async Task Total_Resource_Count_Excluded_From_PATCH_Response()
        {
            // Arrange
            var todoItem = new TodoItem();

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
                    attributes = new
                    {
                        description = "Something else"
                    }
                }
            };

            var route = "/api/v1/todoItems/" + todoItem.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Meta.Should().BeNull();
        }
    }
}
