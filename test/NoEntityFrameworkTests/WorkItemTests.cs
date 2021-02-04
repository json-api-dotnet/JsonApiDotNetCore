using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using NoEntityFrameworkExample;
using NoEntityFrameworkExample.Data;
using NoEntityFrameworkExample.Models;
using TestBuildingBlocks;
using Xunit;

namespace NoEntityFrameworkTests
{
    public sealed class WorkItemTests : IClassFixture<RemoteIntegrationTestContext<Startup, AppDbContext>>
    {
        private readonly RemoteIntegrationTestContext<Startup, AppDbContext> _testContext;

        public WorkItemTests(RemoteIntegrationTestContext<Startup, AppDbContext> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Can_get_WorkItems()
        {
            // Arrange
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(new WorkItem());
                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Can_get_WorkItem_by_ID()
        {
            // Arrange
            var workItem = new WorkItem();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(workItem);
                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/workItems/" + workItem.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(workItem.StringId);
        }

        [Fact]
        public async Task Can_create_WorkItem()
        {
            // Arrange
            var newTitle = Guid.NewGuid().ToString();

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    attributes = new
                    {
                        title = newTitle,
                        ordinal = 1
                    }
                }
            };

            var route = "/api/v1/workItems/";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Attributes["title"].Should().Be(newTitle);
        }

        [Fact]
        public async Task Can_delete_WorkItem()
        {
            // Arrange
            var workItem = new WorkItem();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(workItem);
                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/workItems/" + workItem.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();
        }
    }
}
